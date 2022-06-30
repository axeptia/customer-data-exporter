using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp.DbCommunication
{
    public class MssqlDbCommunication : IDbCommunication
    {
        private readonly ILogger<MssqlDbCommunication> _logger;

        public MssqlDbCommunication(ILogger<MssqlDbCommunication> logger)
        {
            _logger = logger;
        }
        public async Task<List<dynamic>> GetRecords(Dictionary<string, string> configSections)
        {
            var connectionString = configSections[ConfigInfo.Key_ConnectionString];
            var sql = configSections[ConfigInfo.Key_Sql];
            var runFrom = configSections[ConfigInfo.Key_RunFrom].Split("|")[0];

            var sqlParam = GetDefaultSqlParameters();

            if (configSections.TryGetValue(ConfigInfo.Key_SqlParam, out var sqlParamsFromConfig))
            {
                var configSqlParam = sqlParamsFromConfig.Split("|").ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);
                foreach (var param in configSqlParam)
                {
                    if (sqlParam.ContainsKey(param.Key))
                    {
                        sqlParam[param.Key] = param.Value;
                    }
                    else
                    {
                        _logger.LogWarning($"Couldn't use SQL parameter: {param.Key}. It is not a valid parameter name");
                    }
                }
            }

            _logger.LogInformation("Retrieve data from database with following SQL");
            _logger.LogInformation(sql);
            _logger.LogInformation("Parameter @runFrom = " + runFrom);
            var connectionStringWithMaskedPwd = GetMaskedPwdConnectionString(connectionString);
            _logger.LogInformation($"Start executing SQL (connection string: {connectionStringWithMaskedPwd})");

            using var connection = new SqlConnection(connectionString);
            var records = await connection.QueryAsync<dynamic>(sql,
                new
                {
                    runFrom,
                    param1 = sqlParam["param1"],
                    param2 = sqlParam["param2"],
                    param3 = sqlParam["param3"],
                    param4 = sqlParam["param4"],
                    param5 = sqlParam["param5"],
                    param6 = sqlParam["param6"],
                    param7 = sqlParam["param7"],
                    param8 = sqlParam["param8"],
                    param9 = sqlParam["param9"],
                    param10 = sqlParam["param10"],
                });
            var retrievedRecords = records.AsList();
            _logger.LogInformation("Finish executing SQL");
            _logger.LogInformation($"Number of records: {retrievedRecords.Count}");
            return retrievedRecords;
        }

        private Dictionary<string, string> GetDefaultSqlParameters()
        {
            return new Dictionary<string, string>
            {
                { "param1", ""},
                { "param2", ""},
                { "param3", ""},
                { "param4", ""},
                { "param5", ""},
                { "param6", ""},
                { "param7", ""},
                { "param8", ""},
                { "param9", ""},
                { "param10", ""},
            };
        }

        private string GetMaskedPwdConnectionString(string connectionString)
        {
            var maskedCn = connectionString.Split(";").Aggregate((s1, s2) =>
            {
                var s2Washed = s2.TrimStart().ToUpperInvariant().StartsWith("PASSWORD") ? ";Password=******" : s2;
                return $"{s1};{s2Washed}";
            });

            return maskedCn;
        }
    }
}
