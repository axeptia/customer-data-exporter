using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp
{
    public class App
    {
        private readonly Core _core;
        private readonly ConfigFileManager _configFileManager;
        private readonly ILogger<App> _logger;

        public App(Core core, ConfigFileManager configFileManager, ILogger<App> logger)
        {
            _core = core;
            _configFileManager = configFileManager;
            _logger = logger;
        }
        public async Task Run(string[] args, string programBaseDirectory)
        {
            _logger.LogInformation("Program starts");
            if (args.Length == 0)
            {
                _logger.LogInformation("No input argument, program quit");

                var message = "First parameter must be a command parameter" + Environment.NewLine;
                message += GetPossibleCommandParameters();
                Console.Write(message);
                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
                return;
            }

            var configFilePath = $"{programBaseDirectory}/axeptiaExportConfig.txt";
            var dataFileDirectory = programBaseDirectory + "/data";
            var partialConfigsDirectory = programBaseDirectory + "/configs";

            Console.WriteLine("Axeptia exporter Starting");
            Console.WriteLine($"Config path: {configFilePath}");
            Console.WriteLine($"Datafile directory path: {dataFileDirectory}");
            Console.WriteLine($"Partial configuration directory path: {partialConfigsDirectory}");

            var argsMapResult = ProjectArgsIntoMap(args);

            if (argsMapResult.IsFailure)
            {
                Console.WriteLine(argsMapResult.Error);
                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
                return;
            }

            var argsMap = argsMapResult.Value;
            var commandArg = argsMap.Keys.First();

            switch (commandArg)
            {
                case "--version":
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    Console.WriteLine($"Version {version}");
                    Console.WriteLine("Press any key to quit");
                    Console.ReadKey();
                    break;
                case "--run":
                    var valueArg = argsMap[commandArg].Any() ? argsMap[commandArg] : new List<string> { "" };
                    switch (valueArg[0])
                    {
                        case "hello":
                            _logger.LogInformation("Just say hello");
                            Console.WriteLine("Hello");
                            Console.WriteLine("Press any key to quit");
                            Console.ReadKey();
                            break;
                        case "test":
                            await RunTest(configFilePath, dataFileDirectory, partialConfigsDirectory);
                            break;
                        case "real":
                            await RunReal(configFilePath, dataFileDirectory, partialConfigsDirectory);
                            break;
                        default:
                            Console.WriteLine(GettRequiredRunValueArgs());
                            Console.WriteLine();
                            Console.WriteLine("Quit the program by press any key");
                            Console.ReadKey();
                            return;
                    }
                    break;
                default:
                    Console.WriteLine("Program must be started with one of the following parameters");
                    Console.Write(GetPossibleCommandParameters());
                    Console.ReadKey();
                    return;
            }
        }

        private Result<Dictionary<string, List<string>>> ProjectArgsIntoMap(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Arguments is missing", nameof(args));
            }

            var argsMap = new Dictionary<string, List<string>>();
            string argCommand = null;
            var argsValue = new List<string>();

            var argsNr = 0;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    if (argCommand != null)
                    {
                        argsMap.Add(argCommand, argsValue);
                        argsValue = new List<string>();
                    }
                    argCommand = arg;
                }
                else
                {
                    if (argsNr == 0)
                    {
                        var message = "First parameter must be a command parameter" + Environment.NewLine;
                        message += GetPossibleCommandParameters();
                        return Result.Failure<Dictionary<string, List<string>>>(message);
                    }
                    argsValue.Add(arg);
                }
                argsNr++;
            }

            argsMap.Add(argCommand, argsValue);

            return Result.Success(argsMap);

        }

        private string GetPossibleCommandParameters()
        {
            var commandParameters = "--run: Execute the program";
            commandParameters += Environment.NewLine + "--version: Get the program version";
            return commandParameters;
        }

        private async Task RunTest(string configFilePath, string dataFileDirectory, string partialConfigsDirectory)
        {
            await Run(configFilePath, dataFileDirectory, partialConfigsDirectory, false);
        }

        private async Task RunReal(string configFilePath, string dataFileDirectory, string partialConfigsDirectory)
        {
            await Run(configFilePath, dataFileDirectory, partialConfigsDirectory, true);
        }

        private async Task Run(string commonConfigFilePath, string dataFileDirectory, string partialConfigsDirectory, bool isProd)
        {
            _logger.LogInformation($"Start run. Is prod = {isProd}");

            var processCommonConfigFileResult = await ProcessConfigFile(commonConfigFilePath);

            if (processCommonConfigFileResult.IsFailure)
            {
                _logger.LogError("Config file is not properly configured");
                Console.WriteLine(processCommonConfigFileResult.Error);
                Console.WriteLine("Press any key to quit program");
                Console.ReadKey();
                return;
            }

            var partialConfigFileResult = await ProcessPartialConfigFile(partialConfigsDirectory);
            if (partialConfigFileResult.IsFailure)
            {
                _logger.LogError("Partial Config file(s) is not properly configured");
                Console.WriteLine(partialConfigFileResult.Error);
                Console.WriteLine("Press any key to quit program");
                Console.ReadKey();
                return;
            }

            var partialConfigFileContent = partialConfigFileResult.Value;

            var configInfo = new ConfigInfo();
            var confKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, commonConfigFilePath);
            var confContent = processCommonConfigFileResult.Value.First().Value;
            configInfo.AddConfig(confKey, confContent);

            foreach (var key in partialConfigFileContent.Keys)
            {
                confKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, key);
                confContent = partialConfigFileContent[key];
                configInfo.AddConfig(confKey, confContent);
            }

            var validateConfigInfoResult = configInfo.ValidateConfigInfo();

            if (validateConfigInfoResult.IsFailure)
            {
                _logger.LogError("Config file(s) content is not valid");
                Console.WriteLine(validateConfigInfoResult.Error);
                Console.WriteLine("Press any key to quit program");
                Console.ReadKey();
                return;
            }


            if (!isProd)
            {
                configInfo = AdaptConfigToTestRun(configInfo);
            }

            await _core.Execute(configInfo, dataFileDirectory, isProd);
            _logger.LogInformation($"Finish run. Is prod = {isProd}");

        }

        private ConfigInfo AdaptConfigToTestRun(ConfigInfo configinfo)
        {
            var configs = configinfo.Configs;
            foreach (var configKey in configs.Keys)
            {
                if (configs[configKey].ContainsKey(ConfigInfo.Key_Sql))
                {
                    var sqlWhoReturns100Records = SqlStatementHelper.AddLimitToSql(configs[configKey][ConfigInfo.Key_Sql]);
                    configinfo.UpdateConfigSection(configKey, ConfigInfo.Key_Sql, sqlWhoReturns100Records);
                }
            }
            return configinfo;
        }

        private async Task<Result<Dictionary<string, Dictionary<string, string>>>> ProcessConfigFile(string configFilePath)
        {
            var getConfigFileContentResult = await _configFileManager.GetConfigFileContent(configFilePath);
            if (getConfigFileContentResult.IsFailure)
            {
                return Result.Failure<Dictionary<string, Dictionary<string, string>>>(getConfigFileContentResult.Error);
            }
            var configs = _configFileManager.ProjectConfigContentToDictionary(getConfigFileContentResult.Value);

            var configsReturned = new Dictionary<string, Dictionary<string, string>>();
            configsReturned.Add(configFilePath, configs);

            return Result.Success(configsReturned);
        }

        private async Task<Result<Dictionary<string, Dictionary<string, string>>>> ProcessPartialConfigFile(string partialConfigFileDirectory)
        {
            if (!Directory.Exists(partialConfigFileDirectory))
            {
                return Result.Success(new Dictionary<string, Dictionary<string, string>>());
            }
            var partialConfigFiles = Directory.GetFiles(partialConfigFileDirectory, "*_config.txt");

            if (partialConfigFileDirectory.Length == 0)
            {
                _logger.LogInformation($"No partial configuraton found in {partialConfigFileDirectory}");
                return Result.Success(new Dictionary<string, Dictionary<string, string>>());
            }

            _logger.LogInformation($"{partialConfigFileDirectory.Length} partial configuraton found in {partialConfigFileDirectory}");

            var returnedConfigs = new Dictionary<string, Dictionary<string, string>>();

            foreach (var configFilePath in partialConfigFiles)
            {
                var getConfigFileContentResult = await _configFileManager.GetConfigFileContent(configFilePath);
                if (getConfigFileContentResult.IsFailure)
                {
                    return Result.Failure<Dictionary<string, Dictionary<string, string>>>(getConfigFileContentResult.Error);
                }
                var partialConfigs = _configFileManager.ProjectConfigContentToDictionary(getConfigFileContentResult.Value);
                returnedConfigs.Add(configFilePath, partialConfigs);
            }
            return Result.Success(returnedConfigs);
        }



        private string GettRequiredRunValueArgs()
        {
            var paramInfo = "--run required one of the following values" + Environment.NewLine;
            paramInfo += "hello: Will just write hello to the console" + Environment.NewLine;
            paramInfo += "test: Will get 100 records from the SQL set in config file and store it to the file" + Environment.NewLine;
            paramInfo += "real: Will run the program, use info from the configuration";
            return paramInfo;
        }
    }
}
