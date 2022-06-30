using CSharpFunctionalExtensions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxeptiaExporter.ConsoleApp
{
    public class ConfigInfo
    {
        public static string Key_Company = "Company";
        public static string Key_GUID = "GUID";
        public static string Key_Filename = "Filename";
        public static string Key_Sql = "Sql";
        public static string Key_SqlParam = "SqlParam";
        public static string Key_ConnectionString = "ConnectionString";
        public static string Key_RunFrom = "RunFrom";
        public static string Key_RunFromUpdateByColumn = "RunFromUpdateByColumn";
        public static string Key_BlobStorageUrl = "BlobStorageUrl";
        public static string Key_BlobContainer = "BlobContainer";
        public static string Key_MaxRecordsInBatch = "MaxRecordsInBatch";
        public static string Key_ExchangeSasTokenCode = "ExchangeSasTokenCode";
        public static string Key_End = "End";

        public static string RunFromFormat_DateTime = "DateTime";
        public static string RunFromFormat_Number = "Number";
        private static readonly List<string> validRunFromFormats = new List<string> { RunFromFormat_DateTime, RunFromFormat_Number };
        public List<ConfigKey> PartialConfigKeys => Configs.Keys.Where(k => k.Type == ConfigType.Partial).AsList();

        public Dictionary<string, string> CommonConfig => Configs[CommonConfigKey];

        public ConfigKey CommonConfigKey => Configs.Keys.First(k => k.Type == ConfigType.Common);
        public bool ContainsPartialConfigs => Configs.Keys.Any(k => k.Type == ConfigType.Partial);
        public enum ConfigType
        {
            Common,
            Partial
        }

        public class ConfigKey
        {
            public ConfigType Type { get; private set; }
            public string ConfigPath { get; private set; }

            public static ConfigKey Instance(ConfigType type, string configPath)
            {
                return new ConfigKey(type, configPath);
            }
            private ConfigKey(ConfigType type, string configPath)
            {
                Type = type;
                ConfigPath = configPath;
            }
        }

        public Dictionary<ConfigKey, Dictionary<string, string>> Configs { get; }

        public ConfigInfo()
        {
            Configs = new Dictionary<ConfigKey, Dictionary<string, string>>();
        }



        public ConfigInfo AddConfig(ConfigKey configKey, Dictionary<string, string> configs)
        {
            Configs.Add(configKey, configs);
            return this;
        }

        public ConfigInfo UpdateConfigSection(ConfigKey configKey, string sectionKey, string sectionValue)
        {
            if (!Configs.ContainsKey(configKey))
            {
                throw new ArgumentException($"Config does not contain key: {configKey}");
            }

            if (!Configs[configKey].ContainsKey(sectionKey))
            {
                throw new ArgumentException($"Config does not contain section: {sectionKey}");
            }

            Configs[configKey][sectionKey] = sectionValue;

            return this;
        }

        public ConfigInfo UpdateConfigWhenKeyExistElseCommonConfig(ConfigKey configKey, string sectionKey, string sectionValue)
        {
            var curConfigContainsSection = true;

            if (!Configs.ContainsKey(configKey))
            {
                curConfigContainsSection = false;
            }

            if (curConfigContainsSection && !Configs[configKey].ContainsKey(sectionKey))
            {
                curConfigContainsSection = false;
            }

            var configKeyToUse = curConfigContainsSection ? configKey : CommonConfigKey;

            Configs[configKeyToUse][sectionKey] = sectionValue;

            return this;
        }




        internal bool DoConfigContainSection(ConfigKey configKey, string sectionKey)
        {
            if (!Configs.ContainsKey(configKey))
            {
                return false;
            }

            if (!Configs[configKey].ContainsKey(sectionKey))
            {
                return false;
            }
            return true;
        }

        public Dictionary<string, string> MergeWithCommonConfig(ConfigKey partialConfigKey)
        {
            if (partialConfigKey.Type != ConfigType.Partial)
            {
                throw new ArgumentException("Is not Partial ConfigType", nameof(partialConfigKey));
            }

            if (Configs.Keys.Count(c => c.Type == ConfigType.Common) != 1)
            {
                throw new InvalidOperationException("Is not exactly one Common config");
            }

            var commonConfigs = Configs[Configs.Keys.First(ck => ck.Type == ConfigType.Common)];
            var mergedConfigs = new Dictionary<string, string>(commonConfigs);
            var partialConfigs = Configs[partialConfigKey];

            foreach (var partialConfKey in partialConfigs.Keys)
            {
                if (mergedConfigs.ContainsKey(partialConfKey))
                {

                    mergedConfigs[partialConfKey] = partialConfigs[partialConfKey];
                }
                else
                {
                    mergedConfigs.Add(partialConfKey, partialConfigs[partialConfKey]);
                }
            }

            return mergedConfigs;
        }



        private static List<SectionRequirement> SectionRequirements { get; } = new List<SectionRequirement>
        {
            new SectionRequirement{Name = Key_Company, Required = true },
            new SectionRequirement{Name = Key_GUID, Required = true },
            new SectionRequirement{Name = Key_ConnectionString, Required = true },
            new SectionRequirement{Name = Key_Sql, Required = true },
            new SectionRequirement{Name = Key_RunFrom, Required = true },
            new SectionRequirement{Name = Key_RunFromUpdateByColumn, Required = true },
            new SectionRequirement{Name = Key_BlobStorageUrl, Required = true },
            new SectionRequirement{Name = Key_BlobContainer, Required = true },
            new SectionRequirement{Name = Key_ExchangeSasTokenCode, Required = true },
            new SectionRequirement{Name = Key_MaxRecordsInBatch, Required = true },
            new SectionRequirement{Name = Key_End, Required = false },
            new SectionRequirement{Name = Key_SqlParam, Required = false },
            new SectionRequirement{Name = Key_Filename, Required = false },
        };

        public Result ValidateConfigInfo()
        {
            var containPartialConfig = false;
            var commonConfigFileKey = Configs.Keys.First(k => k.Type == ConfigType.Common);
            var commonConfig = Configs[commonConfigFileKey];

            foreach (var partialConfigKey in Configs.Keys.Where(k => k.Type == ConfigType.Partial))
            {
                containPartialConfig = true;
                var mergetConfig = MergeWithCommonConfig(partialConfigKey);

                var validateResult = ValidateConfigInfo(mergetConfig);
                if (validateResult.IsFailure)
                {
                    return Result.Failure($"Validation of combined common config file {commonConfigFileKey} and partial file {partialConfigKey} failed. {validateResult.Error}");
                }
            }

            if (!containPartialConfig)
            {
                var validateResult = ValidateConfigInfo(commonConfig);
                if (validateResult.IsFailure)
                {
                    return Result.Failure($"Validation of common config file {commonConfigFileKey} failed. {validateResult.Error}");
                }
            }
            return Result.Success();
        }

        public Result<Dictionary<string, string>> ValidateConfigInfo(Dictionary<string, string> configs)
        {
            var missingSections = SectionRequirements.Where(sr => sr.Required).Select(sr => sr.Name).ToList().Except(configs.Keys).ToList();
            if (missingSections.Count > 0)
            {
                return BuildMissingSectionFailureResult(missingSections);
            }

            var sectionsWithoutValue = configs.Where(r => string.IsNullOrWhiteSpace(r.Value));
            if (sectionsWithoutValue.Any())
            {
                return BuildMissingRequiredContentFailureResult(sectionsWithoutValue);
            }

            if (!Guid.TryParse(configs[Key_GUID], out var validGuid))
            {
                return Result.Failure<Dictionary<string, string>>($"Your GUID {configs[Key_GUID]} is invalid. You must use a GUID retrieved from Axeptia");
            }

            if (!Guid.TryParse(configs[Key_ExchangeSasTokenCode], out var validSasTokenCodeGuid))
            {
                return Result.Failure<Dictionary<string, string>>($"Your Exchange Sas Token Code {configs[Key_ExchangeSasTokenCode]} is invalid. You must use a GUID retrieved from Axeptia");
            }

            if (!int.TryParse(configs[Key_MaxRecordsInBatch], out var validInt))
            {
                return Result.Failure<Dictionary<string, string>>($"{Key_MaxRecordsInBatch} is invalid. It must be a number");
            }

            var doSqlContainRequiredParameter = configs[Key_Sql].Contains("@runFrom");
            if (!doSqlContainRequiredParameter)
            {
                return Result.Failure<Dictionary<string, string>>($"The sql must contain the @runFrom parameter, which should be used to set the time to retrieve data from. The value that will be passed into this parameter is stored in the {Key_RunFrom} section in the configuration file");
            }

            if (configs[Key_RunFrom].Split("|").Length < 2)
            {
                return Result.Failure<Dictionary<string, string>>("RunFrom is missing format info. RunFrom should be like this [Date]|[Format], and format one of the following: DateTime or Number");
            }


            if (!validRunFromFormats.Contains(configs[Key_RunFrom].Split("|")[1]))
            {
                return Result.Failure<Dictionary<string, string>>("RunFrom format is invalid, must be DateTime or Number");
            }

            return Result.Success(configs);
        }

        private Result<Dictionary<string, string>> BuildMissingSectionFailureResult(List<string> missingSections)
        {
            var failureMessage = "Following sections are missing:";
            foreach (var missingSection in missingSections)
            {
                failureMessage += Environment.NewLine + missingSection;
            }
            return Result.Failure<Dictionary<string, string>>(failureMessage);
        }

        private Result<Dictionary<string, string>> BuildMissingRequiredContentFailureResult(IEnumerable<KeyValuePair<string, string>> sectionsWithoutValue)
        {
            var failureMessage = "Following required sections do not contain any info:";
            foreach (var sectionWithouValue in sectionsWithoutValue)
            {
                failureMessage += Environment.NewLine + sectionWithouValue.Key;
            }
            return Result.Failure<Dictionary<string, string>>(failureMessage);
        }
    }
}
