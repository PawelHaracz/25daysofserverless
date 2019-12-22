using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day22
{
    public class BackupHandlers
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly KeyVaultOptions _options;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private const string BackupUrlTemplate = "https://{0}.vault.azure.net/secrets/{1}/backup?api-version=2016-10-01";
        private const string RestoreUrlTemplate = "https://{0}.vault.azure.net/secrets/restore?api-version=7.0";
        private const string KeyVaultAccessTokenResource = "https://vault.azure.net";
        
        public BackupHandlers(IHttpClientFactory httpClientFactory, IOptions<KeyVaultOptions> options)
        {
            _azureServiceTokenProvider = new AzureServiceTokenProvider();
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        [FunctionName(nameof(Backup))]
        public async Task<IActionResult> Backup(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = nameof(Backup))]
            HttpRequest req,
            IBinder binder,
            CancellationToken cancellationToken,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var accessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(KeyVaultAccessTokenResource, cancellationToken: cancellationToken);
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            
            var tasks = _options.Secrets.Select(Backup);
            await Task.WhenAll(tasks);
            
            return new OkResult();
            
            async Task Backup(string secret)
            {
                var url = string.Format(BackupUrlTemplate, _options.KeyVaultName, secret);
                var backupResponse = await httpClient.PostAsync(url, null, cancellationToken);
                backupResponse.EnsureSuccessStatusCode();
                var model = await backupResponse.Content.ReadAsAsync<BackupModel>(cancellationToken);

                var blobName = $"backups/{DateTime.Now:yy-MM-dd}/{secret}";
                var attribute = new BlobAttribute(blobName, FileAccess.Write)
                {
                    Connection = "AzureWebJobsStorage"
                };

                var byteArray = Encoding.ASCII.GetBytes(model.Value);
                await using var stream = new MemoryStream(byteArray);
                await using var blob = await binder.BindAsync<Stream>(attribute, cancellationToken);
                await stream.CopyToAsync(blob, cancellationToken);
            }
        }

        [FunctionName(nameof(Restore))]
        public async Task<IActionResult> Restore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = nameof(Restore) + "/{date:datetime}")]
            HttpRequest req, 
            DateTime date,
            CancellationToken cancellationToken,
            IBinder binder,
            ILogger log)
        {
         
            log.LogInformation("C# HTTP trigger function processed a request.");
            using var httpClient = _httpClientFactory.CreateClient();
            
            var accessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(KeyVaultAccessTokenResource, cancellationToken: cancellationToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            
            var tasks = _options.Secrets.Select(Restore);

            await Task.WhenAll(tasks);

            return new OkResult();
            
            async Task Restore(string secret)
            {
                var blobName = $"backups/{date:yy-MM-dd}/{secret}";
                var attribute = new BlobAttribute(blobName, FileAccess.Read)
                {
                    Connection = "AzureWebJobsStorage"
                };
                await using var blob = await binder.BindAsync<Stream>(attribute, cancellationToken);
                using var reader = new StreamReader(blob);
                var value = reader.ReadToEnd();

                var url = string.Format(RestoreUrlTemplate, _options.KeyVaultName);
                var model = new BackupModel()
                {
                    Value = value
                };

                var content = new StringContent(JsonConvert.SerializeObject(model, _jsonSerializerSettings),Encoding.Default, "application/json");
                var backupResponse = await httpClient.PostAsync(url, content, cancellationToken);
                backupResponse.EnsureSuccessStatusCode();
            }
        }
        
    }
}