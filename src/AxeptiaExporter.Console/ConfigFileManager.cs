using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp
{
    public class ConfigFileManager
    {
        private static string SectionStartMarker = "|[[";
        private static string SectionEndMarker = "]]|";
        private static string SectionCommentStartMarker = "/***";
        private static string SectionCommentEndMarker = "***/";

        private readonly ILogger<ConfigFileManager> _logger;

        public ConfigFileManager(ILogger<ConfigFileManager> logger)
        {
            _logger = logger;
        }

        

        public async Task<Result<string>> GetConfigFileContent(string pathConfigFile)
        {
            if (!File.Exists(pathConfigFile))
            {
                return Result.Failure<string>($"Config file {pathConfigFile} is missing");
            }

            var fileContent = await File.ReadAllTextAsync(pathConfigFile);
            return Result.Success(fileContent);
        }

        public Dictionary<string, string> ProjectConfigContentToDictionary(string configContent)
        {
            //Read content in section. Mark as |[SECTION]|
            //Content in section is line below |[SECTION]| until line above |[NEXT SECTION]|

            var stopReading = false;
            var caretPos = 0;
            var retrievedConfigFromFile = new Dictionary<string, string>();
            while(!stopReading){
                var startSectionPos = configContent.IndexOf(SectionStartMarker, caretPos);
                var endSectionPos = configContent.IndexOf(SectionEndMarker, caretPos) + SectionEndMarker.Length;
                var sectionName = configContent.Substring(startSectionPos, endSectionPos - startSectionPos);
                var sectionNameWithoutMarkers = sectionName.Replace(SectionStartMarker, "").Replace(SectionEndMarker, "");
                if (sectionNameWithoutMarkers.Trim().ToUpper().Equals("END"))
                {
                    retrievedConfigFromFile.Add(sectionNameWithoutMarkers, "END");
                    break;                        
                }
                caretPos = endSectionPos;
                var sectionContentStartPos = caretPos;
                var sectionContentEndPos = configContent.IndexOf(SectionStartMarker, caretPos);
                caretPos = sectionContentEndPos;
                var sectionContent = configContent.Substring(sectionContentStartPos, sectionContentEndPos - sectionContentStartPos);

                _logger.LogInformation($"Start remove comments for section {sectionNameWithoutMarkers}");
                sectionContent = RemoveComments(sectionContent);
                retrievedConfigFromFile.Add(sectionNameWithoutMarkers, sectionContent.Trim());
            }

            return retrievedConfigFromFile;
        }

        private string RemoveComments(string sectionContent)
        {
            if(sectionContent.Contains(SectionCommentStartMarker) != sectionContent.Contains(SectionCommentEndMarker))
            {
                _logger.LogInformation($"Do not find both start ({SectionCommentStartMarker}) and end ({SectionCommentEndMarker}) comments marker");
                _logger.LogInformation($"Comment start marker found = {sectionContent.Contains(SectionCommentStartMarker)}");
                _logger.LogInformation($"Comment end marker found = {sectionContent.Contains(SectionCommentEndMarker)}");
                throw new InvalidOperationException("Incomplete comment block. Missing start or end marker");
            }

            if (!sectionContent.Contains(SectionCommentStartMarker))
            {
                return sectionContent;
            }

            var caretPos = 0;
            var startSectionCommentPos = sectionContent.IndexOf(SectionCommentStartMarker, caretPos);
            var endSectionCommentPos = sectionContent.IndexOf(SectionCommentEndMarker, caretPos) + SectionCommentEndMarker.Length;
            var sectionContentWithoutComment = sectionContent.Remove(startSectionCommentPos, endSectionCommentPos - startSectionCommentPos);
            return sectionContentWithoutComment;
        }

        

        public async Task<Result> UpdateConfigFile(Dictionary<string,string> newConfigInfo, string pathConfigFile)
        {
            if (!File.Exists(pathConfigFile))
            {
                return Result.Failure($"Config file {pathConfigFile} is missing");
            }

            var fileInfo = new FileInfo(pathConfigFile);
            var backupOfExistingConfigPath = $"{fileInfo.DirectoryName}/{fileInfo.Name}.backup";

            File.Copy(pathConfigFile, backupOfExistingConfigPath);
            File.Delete(pathConfigFile);
            var newConfigFileContent = new StringBuilder();
            foreach (var configInfo in newConfigInfo)
            {
                newConfigFileContent.AppendLine($"|[[{configInfo.Key}]]|");
                newConfigFileContent.AppendLine($"{configInfo.Value}");
                newConfigFileContent.AppendLine();
            }

            await File.WriteAllTextAsync(pathConfigFile, newConfigFileContent.ToString(),Encoding.UTF8);

            File.Delete(backupOfExistingConfigPath);

            return Result.Success();
        }

    }

    class SectionRequirement
    {
        public string Name { get; set; }
        public bool Required { get; set; }
    }
}
