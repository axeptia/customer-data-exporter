using System;

namespace AxeptiaExporter.ConsoleApp
{
    public class SqlStatementHelper
    {
        public static string AddLimitToSql(string sql)
        {
            var searchFor = "SELECT";
            var caretPosAfterSelectStatement = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) + searchFor.Length;
            var sqlPartBeforeCaret = sql.Substring(0, caretPosAfterSelectStatement).Trim();
            var sqlPartAfterCaret = sql.Substring(caretPosAfterSelectStatement, sql.Length - caretPosAfterSelectStatement).Trim();

            var limitsRecordIsAlreadyImplemented = sqlPartAfterCaret.IndexOf("TOP ",StringComparison.OrdinalIgnoreCase) == 0;
            if (limitsRecordIsAlreadyImplemented)
            {
                return sql;
            }
            return $"{sqlPartBeforeCaret} TOP 100 {sqlPartAfterCaret}";      
        }
    }
}
