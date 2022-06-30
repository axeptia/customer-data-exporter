using AxeptiaExporter.ConsoleApp;
using System;
using System.Collections.Generic;
using Xunit;

namespace AxeptiaExporter.Tests
{
    public class ConfigInfoTests
    {
        [Fact]
        public void Verify_Common_Config_Is_Retrieved_When_Asked_For()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.CommonConfig;
            //Assert
            Assert.Equal(2, result.Keys.Count);
            Assert.True(sut.CommonConfigKey.Type == ConfigInfo.ConfigType.Common);
            Assert.Equal("GUID common", result[ConfigInfo.Key_GUID]);
            Assert.Equal("SELECT common", result[ConfigInfo.Key_Sql]);
        }

        [Fact]
        public void Verify_Common_ConfigKey_Is_Retrieved_When_Asked_For()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.CommonConfigKey;
            //Assert
            Assert.True(result.Type == ConfigInfo.ConfigType.Common);
            Assert.Equal("fake_common", result.ConfigPath);
        }

        [Fact]
        public void Verify_Configs_Is_As_Added()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.Configs;
            var resultConfigCommon = result[configKeyCommon];
            var resultConfigPartial1 = result[configKeyPartial1];
            //Assert
            Assert.Equal("GUID partial 1", resultConfigPartial1[ConfigInfo.Key_GUID]);
            Assert.Equal("SELECT partial 1", resultConfigPartial1[ConfigInfo.Key_Sql]);
            Assert.Equal("GUID common", resultConfigCommon[ConfigInfo.Key_GUID]);
            Assert.Equal("SELECT common", resultConfigCommon[ConfigInfo.Key_Sql]);

        }

        [Fact]
        public void Should_Return_True_When_Partial_Config_Exists()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.ContainsPartialConfigs;
            //Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_False_When_Partial_Config_Exists()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.ContainsPartialConfigs;
            //Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_Configs_After_Merged_Partial_Config_With_Common()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.MergeWithCommonConfig(configKeyPartial1);
            //Assert
            Assert.Equal("GUID partial 1", result[ConfigInfo.Key_GUID]);
            Assert.Equal("SELECT partial 1", result[ConfigInfo.Key_Sql]);
            Assert.Equal("Excange code sas token partial 1", result[ConfigInfo.Key_ExchangeSasTokenCode]);
        }

        [Fact]
        public void Should_Verify_Number_Of_Returned_Parial_ConfigKeys_Is_Same_As_Partial_Configs_Added()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            var configKeyPartial2 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configPartial2 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 2" },
                {ConfigInfo.Key_Sql, "SELECT partial 2" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 2" }
            };
            sut.AddConfig(configKeyPartial2, configPartial2);


            //Act
            var result = sut.PartialConfigKeys;
            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Should_Verify_Section_Value_Is_Updated_For_ConfigKey_When_Exsists()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.UpdateConfigSection(configKeyPartial1, ConfigInfo.Key_GUID, "GUID partial 1 updated");
            //Assert
            Assert.Equal("GUID partial 1 updated", result.Configs[configKeyPartial1][ConfigInfo.Key_GUID]);
            Assert.Equal("GUID partial 1 updated", sut.Configs[configKeyPartial1][ConfigInfo.Key_GUID]);
        }

        [Fact]
        public void Should_Throw_Exception_When_ConfigKey_Do_Not_Exists_When_Try_Update_A_Section_For_A_ConfigKey()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configKeyPartial2 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_2");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var exeption = Assert.Throws<ArgumentException>(() => sut.UpdateConfigSection(configKeyPartial2, ConfigInfo.Key_GUID, "GUID partial 1 updated"));
            //Assert
            Assert.StartsWith("Config does not contain key", exeption.Message);
        }

        [Fact]
        public void Should_Throw_Exception_When_Section_Do_Not_Exists_When_Try_Update_A_Section_For_A_Valid_ConfigKey()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var exeption = Assert.Throws<ArgumentException>(() => sut.UpdateConfigSection(configKeyPartial1, ConfigInfo.Key_ConnectionString, "GUID partial 1 updated"));
            //Assert
            Assert.StartsWith("Config does not contain section", exeption.Message);
        }

        [Fact]
        public void Should_Verify_That_Common_Config_Is_Updated_When_Section_Not_Found_In_Given_Partial_Config_Key()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configKeyPartial2 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_2");

            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID partial 1" },
                {ConfigInfo.Key_Sql, "SELECT partial 1" },
                {ConfigInfo.Key_ExchangeSasTokenCode, "Excange code sas token partial 1" }
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_GUID,"GUID common" },
                {ConfigInfo.Key_Sql, "SELECT common" }
            };
            sut.AddConfig(configKeyCommon, configCommon);

            //Act
            var result = sut.UpdateConfigWhenKeyExistElseCommonConfig(configKeyPartial2, ConfigInfo.Key_Sql, "Updated SQL");
            //Assert
            Assert.Equal("Updated SQL", result.CommonConfig[ConfigInfo.Key_Sql]);
            Assert.Equal("Updated SQL", result.Configs[configKeyCommon][ConfigInfo.Key_Sql]);
        }

        [Fact]
        public void Should_Contain_Valid_Config_When_Required_Configs_Is_Splittet_Between_Common_And_One_Partial_Configs()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_Company,"GUID common" },
                {ConfigInfo.Key_GUID, Guid.NewGuid().ToString()},
                {ConfigInfo.Key_ConnectionString,"Fake text" },
                {ConfigInfo.Key_Sql,"Fake text @runFrom" },
                {ConfigInfo.Key_RunFromUpdateByColumn,"Fake text" },
                {ConfigInfo.Key_BlobStorageUrl,"Fake text" },
                {ConfigInfo.Key_BlobContainer,"Fake text" },
                {ConfigInfo.Key_ExchangeSasTokenCode,Guid.NewGuid().ToString() },
                {ConfigInfo.Key_End,"End of file" },
            };
            sut.AddConfig(configKeyCommon, configCommon);

            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime" },
                {ConfigInfo.Key_MaxRecordsInBatch,"100" },

            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            //Act
            var result = sut.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Should_Contain_Valid_Config_When_Required_Configs_Contain_Of_Multiple_Partial_Configs()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_Company,"GUID common" },
                {ConfigInfo.Key_GUID, Guid.NewGuid().ToString()},
                {ConfigInfo.Key_ConnectionString,"Fake text" },
                {ConfigInfo.Key_Sql,"Fake text @runFrom" },
                {ConfigInfo.Key_RunFromUpdateByColumn,"Fake text" },
                {ConfigInfo.Key_BlobStorageUrl,"Fake text" },
                {ConfigInfo.Key_BlobContainer,"Fake text" },
                {ConfigInfo.Key_ExchangeSasTokenCode,Guid.NewGuid().ToString() },
                {ConfigInfo.Key_End,"End of file" },
            };
            sut.AddConfig(configKeyCommon, configCommon);

            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime"},
                {ConfigInfo.Key_MaxRecordsInBatch,"100" },
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configKeyPartial2 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_2");
            var configPartial2 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime"},
                {ConfigInfo.Key_MaxRecordsInBatch,"100" },

            };
            sut.AddConfig(configKeyPartial2, configPartial2);

            //Act
            var result = sut.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Should_Return_Failure_When_Combine_Common_Config_And_One_Partial_Config_Who_Not_Contain_Required_Config_Info()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configKeyCommon = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Common, "fake_common");
            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_Company,"GUID common" },
                {ConfigInfo.Key_GUID, Guid.NewGuid().ToString()},
                {ConfigInfo.Key_ConnectionString,"Fake text" },
                {ConfigInfo.Key_Sql,"Fake text @runFrom" },
                {ConfigInfo.Key_RunFromUpdateByColumn,"Fake text" },
                {ConfigInfo.Key_BlobStorageUrl,"Fake text" },
                {ConfigInfo.Key_BlobContainer,"Fake text" },
                {ConfigInfo.Key_ExchangeSasTokenCode,Guid.NewGuid().ToString() },
                {ConfigInfo.Key_End,"End of file" },
            };
            sut.AddConfig(configKeyCommon, configCommon);

            var configKeyPartial1 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_1");
            var configPartial1 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime"},
                {ConfigInfo.Key_MaxRecordsInBatch,"100" },
            };
            sut.AddConfig(configKeyPartial1, configPartial1);

            var configKeyPartial2 = ConfigInfo.ConfigKey.Instance(ConfigInfo.ConfigType.Partial, "fake_partial_2");
            var configPartial2 = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime"},
                //Missing config section, and do not exists in common either
            };
            sut.AddConfig(configKeyPartial2, configPartial2);

            //Act
            var result = sut.ValidateConfigInfo();
            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Should_Return_True_When_Validate_Valid_Config()
        {
            //Arrange
            var sut = new ConfigInfo();
            var configCommon = new Dictionary<string, string>()
            {
                {ConfigInfo.Key_Company,"GUID common" },
                {ConfigInfo.Key_GUID, Guid.NewGuid().ToString()},
                {ConfigInfo.Key_ConnectionString,"Fake text" },
                {ConfigInfo.Key_Sql,"Fake text @runFrom" },
                {ConfigInfo.Key_RunFromUpdateByColumn,"Fake text" },
                {ConfigInfo.Key_BlobStorageUrl,"Fake text" },
                {ConfigInfo.Key_BlobContainer,"Fake text" },
                {ConfigInfo.Key_ExchangeSasTokenCode,Guid.NewGuid().ToString() },
                {ConfigInfo.Key_End,"End of file" },
                {ConfigInfo.Key_RunFrom,"2020-01-21T14:10:00.000|DateTime"},
                {ConfigInfo.Key_MaxRecordsInBatch,"100" },
            };

            //Act
            var result = sut.ValidateConfigInfo(configCommon);
            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}
