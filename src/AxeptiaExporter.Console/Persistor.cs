using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp
{
    public class Persistor
    {
        private readonly ILogger<Persistor> _logger;

        public Persistor(ILogger<Persistor> logger)
        {
            _logger = logger;
        }
        public async Task<bool> StoreContentToFile(string dataDirectory, List<ContentInfo> contentInfos)
        {
            dataDirectory = dataDirectory.Trim();
            dataDirectory = dataDirectory.EndsWith("/") ? dataDirectory : dataDirectory + "/";

            if (!Directory.Exists(dataDirectory))
            {
                _logger.LogInformation($"Create data directory {dataDirectory}");
                Directory.CreateDirectory(dataDirectory);
            }

            foreach (var contentInfo in contentInfos)
            {
                await File.WriteAllTextAsync($"{dataDirectory}{contentInfo.Filename}", contentInfo.GetContentAsJson(), System.Text.Encoding.UTF8);
                _logger.LogInformation($"Stored content to: {dataDirectory}{contentInfo.Filename}");
                _logger.LogInformation($"Number of recored stored: {contentInfo.Content.Count}");
            }

            return contentInfos.Count > 0;
        }
    }
}
