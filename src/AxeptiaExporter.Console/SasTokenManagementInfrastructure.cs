using AxeptiaExporter.ConsoleApp.Extensions;
using AxeptiaExporter.ConsoleApp.ViewModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace AxeptiaExporter.ConsoleApp
{


    public class SasTokenManagementInfrastructure
    {
        private class HttpSystemContentErrorResponse
        {
            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        HttpClient _httpClient;
        private readonly ILogger<SasTokenManagementInfrastructure> _logger;

        public SasTokenManagementInfrastructure(IHttpClientFactory httpClientFactory, ILogger<SasTokenManagementInfrastructure> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiManagement");
            _logger = logger;
        }


        public async Task<Result<ExchangedCodeToSasTokenViewModel>> ExchangeCodeToSasToken(Guid customerIntegrationConfigId, Guid exchangeCode)
        {

            var queryString = HttpUtility.ParseQueryString($"customerIntegrationConfigId={customerIntegrationConfigId}&exchangecode={exchangeCode}");
            var request = "/data-exporter-exchange-code-to-sas-token/ExchangeCodeToSasToken?" + queryString;
            var httpResponse = await _httpClient.GetAsync(request);

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await httpResponse.Content.ReadAsAsync<ExchangedCodeToSasTokenViewModel>();
                return Result.Success(result);
            }

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var exceptionContent = await httpResponse.Content.ReadAsAsync<HttpSystemContentErrorResponse>();
                return Result.Failure<ExchangedCodeToSasTokenViewModel>($"Following error occured when try exchange code to sas token: {exceptionContent.Message} ({exceptionContent.StatusCode})");
            }

            var exceptionString = await httpResponse.Content.ReadAsStringAsync();
            return Result.Failure<ExchangedCodeToSasTokenViewModel>($"Following error occured when try exchange code to sas token: {exceptionString}");
        }

        public async Task ConfirmToken(Guid customerIntegrationConfigId, Guid exchangeCode)
        {
            try
            {
                var queryString = HttpUtility.ParseQueryString($"customerIntegrationConfigId={customerIntegrationConfigId}&exchangecode={exchangeCode}");
                var request = "/data-exporter-exchange-code-to-sas-token/ConfirmExchangeCodeReceived?" + queryString;
                var result = await _httpClient.GetAsync(request);
            }
            catch (Exception)
            {
                //Just log the error and go on. This should not be a show stopper. 
                //When successful it ensure to invalidate other code.
                //When fail the previous code will still be valid until another code is used, so a failed request do not need to be handled
                _logger.LogError($"Could not verify the new exchange sas code '{exchangeCode}' that was received");
            }
        }
    }
}
