using AxeptiaExporter.ConsoleApp;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AxeptiaExporter.Tests
{
    public class SqlStatementTests
    {
        [Fact]
        public void Should_Add_Limit_To_Sql()
        {
            //Arrange
            var sql = "   SELECT    * FROM table t WHERE t.info = SELECT THINGS    ";
            var expectedResult = "SELECT TOP 100 * FROM table t WHERE t.info = SELECT THINGS";

            //Act
            var result = SqlStatementHelper.AddLimitToSql(sql);

            //Assert
            Assert.Equal(expectedResult,result);
        }

        [Fact]
        public void Should_Not_Add_Limit_To_Sql()
        {
            //Arrange
            var sql = "   SELECT    TOP 100 * FROM table t WHERE t.info = SELECT THINGS    ";
            var expectedResult = "   SELECT    TOP 100 * FROM table t WHERE t.info = SELECT THINGS    ";

            //Act
            var result = SqlStatementHelper.AddLimitToSql(sql);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
