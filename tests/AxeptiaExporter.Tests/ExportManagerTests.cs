using AxeptiaExporter.ConsoleApp;
using AxeptiaExporter.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AxeptiaExporter.Tests
{
    public class ExportManagerTests
    {
        [Fact]
        public void Should_Return_1_File_After_Add_Content()
        {
            //Arrange
            var sut = new ExportManager("01d5d414-f173-4562-9b64-4475d4a6b10c_axeptia",1);
            var maxNrOfContentEachFile = 1;
            //Act
            var result = sut.AddContent(new List<ContentInfo>(), new { company = "Test" }, maxNrOfContentEachFile);
            //Assert
            Assert.Single(result);
            Assert.Contains("_1_1_", result[0].Filename);
        }

        [Fact]
        public void Should_Return_2_Files_After_Add_Content()
        {
            //Arrange
            var sut = new ExportManager("01d5d414-f173-4562-9b64-4475d4a6b10c_axeptia",1);
            var maxNrOfContentEachFile = 1;
            //Act
            var exportInfos = sut.AddContent(new List<ContentInfo>(), new { company = "Test" }, maxNrOfContentEachFile);
            var result = sut.AddContent(exportInfos, new { company = "Test2", b = "" }, maxNrOfContentEachFile);
            //Assert
            Assert.Equal(2,result.Count);
            Assert.Contains("_1_1_", result[0].Filename);
            Assert.Contains("_1_2_", result[1].Filename);
        }

        [Fact]
        public void Should_Serialize_Content_As_Json()
        {
            //Arrange

            var sut = new ExportManager("01d5d414-f173-4562-9b64-4475d4a6b10c_axeptia",1);
            var maxNrOfContentEachFile = 4;
            //Act
            var exportInfos = sut.AddContent(new List<ContentInfo>(), new { Company = "Test", Address = "Test", ZipCode = 0890 }, maxNrOfContentEachFile);
            exportInfos = sut.AddContent(exportInfos, new { Company = "Test2", ZipCode = 1289 }, maxNrOfContentEachFile);
            var jsonContent = exportInfos[0].GetContentAsJson();

            var result = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);
            //Assert
            Assert.Equal("Test",(string)result[0].Company);
            Assert.Equal("Test", (string)result[0].Address);
            Assert.Equal(0890, (int)result[0].ZipCode);
            Assert.Equal("Test2", (string)result[1].Company);
            Assert.Null((string)result[1].Address);
            Assert.Equal(1289, (int)result[1].ZipCode);
        }
    }
}
