using AxeptiaExporter.ConsoleApp.DbCommunication;
using AxeptiaExporter.ConsoleApp.ViewModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;

namespace AxeptiaExporter.ConsoleApp
{
    public class Core
    {
        private readonly IDbCommunication _dbCommunication;
        private readonly ConfigFileManager _configFileManager;
        private readonly Persistor _persistor;
        private readonly BlobUploader _blobUploader;
        private readonly SasTokenManagementInfrastructure _sasTokenManagement;
        private readonly ILogger<Core> _logger;
        private readonly Dictionary<string, ExchangedCodeToSasTokenViewModel> _sasTokenCache = new Dictionary<string, ExchangedCodeToSasTokenViewModel>();

        public Core(IDbCommunication dbCommunication, ConfigFileManager configFileManager,
            Persistor persistor, BlobUploader blobUploader, SasTokenManagementInfrastructure sasTokenManagement, ILogger<Core> logger)
        {
            _dbCommunication = dbCommunication;
            _configFileManager = configFileManager;
            _persistor = persistor;
            _blobUploader = blobUploader;
            _sasTokenManagement = sasTokenManagement;
            _logger = logger;
        }

        internal async Task Execute(ConfigInfo configInfo, string dataFileDirectory, bool isProd)
        {
            var batchNr = 1;

            var configKeysToUse = configInfo.ContainsPartialConfigs ? configInfo.PartialConfigKeys : new List<ConfigInfo.ConfigKey> { configInfo.CommonConfigKey };

            foreach (var exportConfigKey in configKeysToUse)
            {
                var executeResult = await Execute(configInfo, exportConfigKey, dataFileDirectory, batchNr, isProd);

                if (executeResult.IsFailure)
                {
                    throw new InvalidOperationException(executeResult.Error);
                }
                configInfo = executeResult.Value;
                batchNr++;
            }

            if (isProd)
            {
                //Update config file(s) with info as in the config dictionary
                await UpdateConfigFiles(configInfo);
                await ConfirmSasTokenExchangeCodes(configInfo);
            }
        }

        private async Task<Result<ConfigInfo>> Execute(ConfigInfo configInfo, ConfigInfo.ConfigKey configKey, string dataFileDirectory,
            int batchNr, bool isProd)
        {
            ConfigInfo returnedConfigInfo = configInfo;
            var configSections = configInfo.ContainsPartialConfigs ? configInfo.MergeWithCommonConfig(configKey) : configInfo.CommonConfig;

            var contentToExport = await _dbCommunication.GetRecords(configSections);


            var filenamePreFix = configSections.ContainsKey(ConfigInfo.Key_Filename) ? configSections[ConfigInfo.Key_Filename] :
                $"{Guid.Parse(configSections[ConfigInfo.Key_GUID])}_{configSections[ConfigInfo.Key_Company]}";

            var exportManager = new ExportManager(filenamePreFix, batchNr);

            var exportInfos = new List<ContentInfo>();
            foreach (var content in contentToExport)
            {
                exportInfos = exportManager.AddContent(exportInfos, content, int.Parse(configSections[ConfigInfo.Key_MaxRecordsInBatch]));
            }

            var isContentFilesCreated = await _persistor.StoreContentToFile(dataFileDirectory, exportInfos);

            if (!isProd)
            {
                return returnedConfigInfo;
            }

            _logger.LogInformation("Is running in production");

            var newRunFromInfo = GetNewRunFromInfo(configSections, exportInfos);
            var sectionKey = ConfigInfo.Key_RunFrom;
            returnedConfigInfo = configInfo.UpdateConfigWhenKeyExistElseCommonConfig(configKey, sectionKey, newRunFromInfo);
            var configContainsSection = configInfo.DoConfigContainSection(configKey, sectionKey);
            _logger.LogInformation($"Update RunFrom config {(configContainsSection ? configKey.ConfigPath : configInfo.CommonConfigKey.ConfigPath)} section to {newRunFromInfo}");

            var shouldTokenBeRefreshed = true;
            if (_sasTokenCache.TryGetValue(configSections[ConfigInfo.Key_GUID], out ExchangedCodeToSasTokenViewModel exchangeCodeToSasTokenInfo))
            {
                //If token expire within 30 minutes, renew, else use existing.
                shouldTokenBeRefreshed = !IsTokenValid(exchangeCodeToSasTokenInfo.SasToken, DateTime.UtcNow.AddMinutes(30));
            }

            if (shouldTokenBeRefreshed)
            {
                var exchangeCodeToTokenResult = await ExchangeCodeToTokenResult(configSections);

                if (exchangeCodeToTokenResult.IsFailure)
                {
                    _logger.LogError($"Something bad happened when trying to get a new SAS Token, message: {exchangeCodeToTokenResult.Error}");
                    _logger.LogWarning("Program finished without uploading all or some of the files to Axeptia");
                    return Result.Failure<ConfigInfo>(exchangeCodeToTokenResult.Error);
                }

                exchangeCodeToSasTokenInfo = exchangeCodeToTokenResult.Value;

                _sasTokenCache.Add(configSections[ConfigInfo.Key_GUID], exchangeCodeToSasTokenInfo);

                var newSasExchangeCode = exchangeCodeToSasTokenInfo.NewExchangeCode;
                sectionKey = ConfigInfo.Key_ExchangeSasTokenCode;
                returnedConfigInfo = configInfo.UpdateConfigWhenKeyExistElseCommonConfig(configKey, sectionKey, newSasExchangeCode);
                configContainsSection = configInfo.DoConfigContainSection(configKey, sectionKey);
                _logger.LogInformation($"Update ExchangeSasTokenCode config  {(configContainsSection ? configKey.ConfigPath : configInfo.CommonConfigKey.ConfigPath)} section to {newSasExchangeCode}");
            }

            if (!isContentFilesCreated)
            {
                _logger.LogInformation("No content to export, create _pulse file instead");
                //Create _pulse file, so something will be sendt to inform that this job is running as normal
                exportInfos = exportManager.AddPulseContent();
                await _persistor.StoreContentToFile(dataFileDirectory, exportInfos);
                _logger.LogInformation("Pulse file created");
            }
            _logger.LogInformation("Start uploading files to Axeptia");

            await _blobUploader.UploadDataFilesThenDeleteUploadedFile(dataFileDirectory, configSections, exchangeCodeToSasTokenInfo.SasToken);
            _logger.LogInformation("Finish uploading files to Axeptia");
            return returnedConfigInfo;
        }

        private static bool IsTokenValid(string sasTokenQyery, DateTime expireAfterTimeUtc)
        {
            string expireTimeParamValue = HttpUtility.ParseQueryString(sasTokenQyery).Get("se");

            var tokenExpireTimeUtc = DateTimeOffset.Parse(expireTimeParamValue).UtcDateTime;

            return tokenExpireTimeUtc > expireAfterTimeUtc;

        }


        private async Task UpdateConfigFiles(ConfigInfo configInfo)
        {
            foreach (var config in configInfo.Configs)
            {
                await _configFileManager.UpdateConfigFile(config.Value, config.Key.ConfigPath);
            }
        }
        private async Task ConfirmSasTokenExchangeCodes(ConfigInfo configInfo)
        {
            var configSectionSasTokenCode = ConfigInfo.Key_ExchangeSasTokenCode;
            var configSectionCustomerConfigGuid = ConfigInfo.Key_GUID;

            foreach (var config in configInfo.Configs)
            {
                if (config.Value.TryGetValue(configSectionSasTokenCode, out var exchangeCode) &&
                    config.Value.TryGetValue(configSectionCustomerConfigGuid, out var customerIntegrationConfigId))
                {
                    _logger.LogInformation($"Confirm new excange code {exchangeCode} received for integraton {customerIntegrationConfigId}. Located in config {config.Key.ConfigPath}  ");
                    await _sasTokenManagement.ConfirmToken(Guid.Parse(customerIntegrationConfigId), Guid.Parse(exchangeCode));
                }
            }

        }

        private async Task<Result<ExchangedCodeToSasTokenViewModel>> ExchangeCodeToTokenResult(Dictionary<string, string> configSections)
        {
            var customerIntegrationConfigId = Guid.Parse(configSections[ConfigInfo.Key_GUID]);
            var exchangeCode = Guid.Parse(configSections[ConfigInfo.Key_ExchangeSasTokenCode]);
            return await _sasTokenManagement.ExchangeCodeToSasToken(customerIntegrationConfigId, exchangeCode);
        }

        private string GetNewRunFromInfo(Dictionary<string, string> configSections, List<ContentInfo> exportInfos)
        {

            var timeNewestExportedContent = ExtractLatestTimeFromExportedContent(exportInfos, configSections);
            var runFromFormat = configSections[ConfigInfo.Key_RunFrom].Split("|")[1];
            var newConfigInfoRunFrom = $"{timeNewestExportedContent}|{runFromFormat}";
            return newConfigInfoRunFrom;
        }

        public string ExtractLatestTimeFromExportedContent(List<ContentInfo> exportedContent, Dictionary<string, string> configSections)
        {
            var runFromFormat = configSections[ConfigInfo.Key_RunFrom].Split("|")[1];
            var runFromInfoFoundInColumn = configSections[ConfigInfo.Key_RunFromUpdateByColumn];
            var newRunFromValue = configSections[ConfigInfo.Key_RunFrom].Split("|")[0];

            foreach (var contentInfo in exportedContent)
            {

                foreach (var content in contentInfo.Content)
                {
                    try
                    {
                        var details = (IDictionary<string, object>)content;
                        var tmpNewRunFromValue = details[runFromInfoFoundInColumn].ToString();
                        newRunFromValue = KeepNewestRunFromValue(newRunFromValue, tmpNewRunFromValue, runFromFormat);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException($"Could not retrieve new RunFrom value, check if column '{runFromInfoFoundInColumn}' to extract info from is correct and that '{runFromFormat}' is correct");
                    }
                }
            }
            return newRunFromValue;
        }



        public string KeepNewestRunFromValue(string existingNewestRunFromValue, string possibleNewRunFromValue, string runFromFormat)
        {
            if (runFromFormat.Equals(ConfigInfo.RunFromFormat_DateTime))
            {
                try
                {
                    var expectedReturnedDateFormats = new string[]
                        {
                            "yyyy-MM-ddTHH:mm:ss.fff",
                            "yyyy-MM-ddTHH:mm:ss.ff",
                            "yyyy-MM-ddTHH:mm:ss.f",
                            "yyyy-MM-ddTHH:mm:ss",
                        };

                    DateTime.TryParseExact(possibleNewRunFromValue, expectedReturnedDateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var possibleNewRunFromDate);

                    DateTime.TryParseExact(existingNewestRunFromValue, expectedReturnedDateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var existingNewestRunFromDate);
            
                    return possibleNewRunFromDate > existingNewestRunFromDate ? possibleNewRunFromDate.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) : existingNewestRunFromDate.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not convert one of existing: {existingNewestRunFromValue} or new: {possibleNewRunFromValue} to datetime in ISO 8601. Exeption {ex}");
                    return existingNewestRunFromValue;
                }
            }

            if (runFromFormat.Equals(ConfigInfo.RunFromFormat_Number))
            {
                return int.Parse(possibleNewRunFromValue) > int.Parse(existingNewestRunFromValue) ? possibleNewRunFromValue : existingNewestRunFromValue;
            }

            return existingNewestRunFromValue;
        }
    }
}
