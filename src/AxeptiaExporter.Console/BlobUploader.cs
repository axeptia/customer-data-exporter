using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp
{
    public class BlobUploader
    {
        private readonly ILogger<Blob> _logger;

        public BlobUploader(ILogger<Blob> logger)
        {
            _logger = logger;
        }

        internal async Task UploadDataFilesThenDeleteUploadedFile(string dataFileDirectory, Dictionary<string, string> configSections, string sasToken)
        {
            var blobSasUri = $"{configSections[ConfigInfo.Key_BlobStorageUrl]}?{sasToken}";

            var sasUri = new UriBuilder(blobSasUri);

            // Create a client that can authenticate with the SAS URI
            var service = new BlobServiceClient(sasUri.Uri);
            var containerClient = service.GetBlobContainerClient(configSections[ConfigInfo.Key_BlobContainer]);

            var filesToUpload = Directory.GetFiles(dataFileDirectory);
            var numberOfFilesToUpload = filesToUpload.Length;

            _logger.LogInformation($"Start uploading {numberOfFilesToUpload} file(s) to container {containerClient.Uri.ToString().Replace(containerClient.Uri.Query, "")}");
            foreach (var file in filesToUpload)
            {
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name;
                using (FileStream fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    _logger.LogInformation($"Uploading content of {file} to blob {containerClient.Uri.ToString().Replace(containerClient.Uri.Query, "")}/{fileName}");
                    var res = await containerClient.UploadBlobAsync(fileName, fsSource);
                }
                //UploadBlobAsync throws an exception if the upload fails, should be fine to delete file if we get here
                File.Delete(file);
                _logger.LogInformation($"Delete file {file}");
            }
            _logger.LogInformation($"Finish uploading {numberOfFilesToUpload} file(s) to container {containerClient.Uri.ToString().Replace(containerClient.Uri.Query, "")}");
        }
    }
}
