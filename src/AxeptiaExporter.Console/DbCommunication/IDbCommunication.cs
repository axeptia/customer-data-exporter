using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp.DbCommunication
{
    public interface IDbCommunication
    {
        Task<List<dynamic>> GetRecords(Dictionary<string, string> configSections);
    }
}
