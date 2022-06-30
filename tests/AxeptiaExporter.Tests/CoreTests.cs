using AxeptiaExporter.ConsoleApp;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AxeptiaExporter.Tests
{
    public class CoreTests
    {
        [Fact]
        public void Should_Return_New_RunFrom_Value_When_Newer_By_Date()
        {
            //Arrange
            var sut = new Core(null, null, null, null, null, null);
            var existingValue = "2020-06-24T01:01:00.000";
            var newValue = "2021-06-24T01:01:00.000";

            //Act
            var result = sut.KeepNewestRunFromValue(existingValue, newValue, ConfigInfo.RunFromFormat_DateTime);
            //Assert
            Assert.Equal(newValue, result);
        }

        [Fact]
        public void Should_Return_New_RunFrom_Value_When_Newer_By_Number()
        {
            //Arrange
            var sut = new Core(null, null, null, null, null, null);
            var existingValue = "0";
            var newValue = "20171012";

            //Act
            var result = sut.KeepNewestRunFromValue(existingValue, newValue, ConfigInfo.RunFromFormat_Number);
            //Assert
            Assert.Equal(newValue, result);
        }

        [Fact]
        public void Should_Return_Existing_RunFrom_Value_When_Newer_By_Date()
        {
            //Arrange
            var sut = new Core(null, null, null, null, null, null);
            var existingValue = "2022-06-24T01:01:00.000";
            var newValue = "2021-06-24T01:01:00.000";

            //Act
            var result = sut.KeepNewestRunFromValue(existingValue, newValue, ConfigInfo.RunFromFormat_DateTime);
            //Assert
            Assert.Equal(existingValue, result);
        }

        [Fact]
        public void Should_Return_Existing_RunFrom_Value_When_Newer_By_Number()
        {
            //Arrange
            var sut = new Core(null, null, null, null, null, null);
            var existingValue = "20171012";
            var newValue = "20161012";

            //Act
            var result = sut.KeepNewestRunFromValue(existingValue, newValue, ConfigInfo.RunFromFormat_Number);
            //Assert
            Assert.Equal(existingValue, result);
        }

        [Fact(Skip = "Can't solve DataRow")]
        public void Should_Keep_RunFrom_From_Config_When_There_Is_No_Newer_Update_Date()
        {
            //Arrange
            var exportManager = new ExportManager($"{Guid.Empty}_company", 1);
            var contentInfos = exportManager.AddContent(new List<ContentInfo>(), new { Comany = "A", Updated = 20100101 }, 4);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20110101 }, 4);
            var configs = new Dictionary<string, string>
            {
                {ConfigInfo.Key_RunFrom,"20201010|Number"},
                {ConfigInfo.Key_RunFromUpdateByColumn,"Updated"},
            };
            var sut = new Core(null, null, null, null, null, null);

            //Act
            var result = sut.ExtractLatestTimeFromExportedContent(contentInfos, configs);
            //Assert
            Assert.Equal("20201010", result);
        }

        [Fact(Skip = "Can't solve DataRow")]
        public void Should_Get_New_RunFrom_Value_From_Content_Update_Date()
        {
            //Arrange
            var exportManager = new ExportManager($"{Guid.Empty}_company", 1);
            var contentInfos = exportManager.AddContent(new List<ContentInfo>(), new { Comany = "A", Updated = 20100101 }, 4);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20110101 }, 4);

            var configs = new Dictionary<string, string>
            {
                {ConfigInfo.Key_RunFrom,"20051010|Number"},
                {ConfigInfo.Key_RunFromUpdateByColumn,"Updated"},
            };
            var sut = new Core(null, null, null, null, null, null);

            //Act
            var result = sut.ExtractLatestTimeFromExportedContent(contentInfos, configs);
            //Assert
            Assert.Equal("20110101", result);
        }

        [Fact(Skip = "Can't solve DataRow")]
        public void Should_Get_New_RunFrom_Value_From_Content_Update_Date_First_Row()
        {
            //Arrange
            var exportManager = new ExportManager($"{Guid.Empty}_company", 1);
            var contentInfos = exportManager.AddContent(new List<ContentInfo>(), new { Comany = "A", Updated = 20110101 }, 4);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20100101 }, 4);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20090101 }, 4);
            var configs = new Dictionary<string, string>
            {
                {ConfigInfo.Key_RunFrom,"20001010|Number"},
                {ConfigInfo.Key_RunFromUpdateByColumn,"Updated"},
            };
            var sut = new Core(null, null, null, null, null, null);

            //Act
            var result = sut.ExtractLatestTimeFromExportedContent(contentInfos, configs);
            //Assert
            Assert.Equal("20110101", result);
        }

        [Fact(Skip = "Can't solve DataRow")]
        public void Should_Get_New_RunFrom_Value_From_Content_Update_Date_Multiple_Content_Files()
        {
            //Arrange
            var exportManager = new ExportManager($"{Guid.Empty}_company", 1);
            var contentInfos = exportManager.AddContent(new List<ContentInfo>(), new { Comany = "A", Updated = 20110101 }, 1);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20100101 }, 1);
            contentInfos = exportManager.AddContent(contentInfos, new { Comany = "B", Updated = 20090101 }, 1);
            var configs = new Dictionary<string, string>
            {
                {ConfigInfo.Key_RunFrom,"20001010|Number"},
                {ConfigInfo.Key_RunFromUpdateByColumn,"Updated"},
            };
            var sut = new Core(null, null, null, null, null, null);

            //Act
            var result = sut.ExtractLatestTimeFromExportedContent(contentInfos, configs);
            //Assert
            Assert.Equal("20110101", result);
        }
    }
}
