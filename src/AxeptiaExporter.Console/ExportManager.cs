using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AxeptiaExporter.ConsoleApp
{
    public class ExportManager
    {
        private readonly string _fileName;
        private readonly int _batchNr;

        public ExportManager(string fileName, int batchNr)
        {
            _fileName = fileName;
            _batchNr = batchNr;
        }
        public List<ContentInfo> AddContent(List<ContentInfo> exportInfos, dynamic content, int maxNrOfContentEachFile)
        {
            if(exportInfos.Count == 0)
            {
                var fileName = NewFileName(exportInfos.Count+1);
                exportInfos.Add(new ContentInfo(fileName,new List<dynamic>()));
            }

            var lastExportInfo = exportInfos[^1];
            if(lastExportInfo.Content.Count == maxNrOfContentEachFile)
            {

                var fileName = NewFileName(exportInfos.Count + 1);
                lastExportInfo = new ContentInfo(fileName, new List<dynamic>());
                exportInfos.Add(lastExportInfo);
            }
            lastExportInfo.Content.Add(content);
            return exportInfos;
        }

        public List<ContentInfo> AddPulseContent()
        {
            var exportInfos = new List<ContentInfo>();
            var pulseFileName = $"{_fileName}_pulse.json";
            exportInfos.Add(new ContentInfo(pulseFileName, new List<dynamic>()));
            return exportInfos;
        }


        private string NewFileName(int fileNumber)
        {
            var stamp = DateTime.UtcNow.Ticks.ToString();
            var exportDestinationFile = $"{_fileName}_{_batchNr}_{fileNumber}_{stamp}.json";
            return exportDestinationFile;
        }
    }

    public struct ContentInfo
    {
        public string Filename { get; }
        public List<dynamic> Content { get; }

        public ContentInfo(string filename, List<dynamic> content)
        {
            Filename = filename;
            Content = content;
        }

        public string GetContentAsJson()
        {
            var storedContent = JsonSerializer.Serialize(Content);
            return storedContent;
        }
    }
}
