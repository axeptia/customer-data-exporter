using System.Text.Json.Serialization;

namespace AxeptiaExporter.ConsoleApp.ViewModel
{
    public class ExchangedCodeToSasTokenViewModel
    {
        [JsonPropertyName("sasToken")]
        public string SasToken { get; set; }
        [JsonPropertyName("newExchangeCode")]
        public string NewExchangeCode { get; set; }
        [JsonPropertyName("newExchangeCodeWasSuccessfullyGenerated")]
        public bool NewExchangeCodeWasSuccessfullyGenerated { get; set; }
    }
}
