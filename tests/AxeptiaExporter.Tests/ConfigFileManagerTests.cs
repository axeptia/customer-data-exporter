using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace AxeptiaExporter.ConsoleApp
{
    public class ConfigFileManagerTests
    {
        [Fact]
        public void Should_Success_When_All_Required_Section_Is_Set()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithAllRequiredSection.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);

            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);

            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Should_Failure_When_Not_All_Required_Section_Is_Set()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithMissingSection.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("sections are missing", result.Error);
        }

        [Fact]
        public void Should_Failure_When_All_Required_Section_Is_Set_But_Some_Do_Not_Contain_Information()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithSectionWithoutValue.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("sections do not contain any info", result.Error);
        }

        [Fact]
        public void Should_Failure_When_Sql_Do_Not_Contain_Required_Parameter()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithAllRequiredSectionButMissingSqlParameter.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("@runFrom parameter", result.Error);
        }

        [Fact]
        public void Should_Failure_When_Invalid_Guid()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithInvalidGuid.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("GUID", result.Error);
        }

        [Fact]
        public void Should_Failure_When_Invalid_MaxRecordsInBatch()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configWithInvalidMaxRecordsInBatch.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("must be a number", result.Error);
        }

        [Fact]
        public void Should_Failure_When_Invalid_RunFrom_Format()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configInvalidRunFromFormat.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("DateTime or Number", result.Error);
        }

        [Fact]
        public void Should_Failure_When_Missing_RunFrom_Format()
        {
            //Arrange
            var configContent = File.ReadAllText("./testfiles/configMissingRunFromFormat.txt");
            var configManager = new ConfigFileManager(NullLogger<ConfigFileManager>.Instance);
            //Act
            var configs = configManager.ProjectConfigContentToDictionary(configContent);
            var configInfo = new ConfigInfo();
            var configKey = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "no existing path");
            configInfo.AddConfig(configKey, configs);
            var result = configInfo.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("missing format info", result.Error);
        }
    }
}
