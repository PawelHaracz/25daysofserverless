using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Day3.Extensions;
using Day3.Model;
using Day3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Day3.Handlers
{
    public class ThirdDay
    {
        private readonly IPngConverter _pngConverter;
        private readonly IPngService _pngService;
        private readonly JsonSerializerSettings _deserializerSettings;
        private const string GitHubEventName = "X-GitHub-Event";
        private const string DownloadPng = "download-png";
        private const string PetsTable = "Pets";

        public ThirdDay(IPngConverter pngConverter, IPngService pngService)
        {
            _pngConverter = pngConverter;
            _pngService = pngService;
            _deserializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        [FunctionName(nameof(StoreGitPng))]
        public async Task StoreGitPng(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [Queue(DownloadPng, Connection = "AzureWebJobsStorage")]
            IAsyncCollector<DownloadPngMessage> messages,
            ILogger log)

        {
            var correlationId = Guid.NewGuid().ToString("N");
            var cancellationToken = new CancellationToken();
            using (log.BeginScope(new Dictionary<string, object>() {[nameof(correlationId)] = correlationId}))
            {
                var @event = req.Headers.GetEnum<GitHubEvents>(GitHubEventName);
                if (@event is GitHubEvents.Ping || @event is GitHubEvents.Unknown)
                {
                    log.LogInformation("Incorrect event type : {eventType}", @event);
                    return;
                }

                var content = await req.ReadAsStringAsync();
                var commitEvent = JsonConvert.DeserializeObject<GitCommitEvent>(content, _deserializerSettings);

                var tasks = _pngConverter.ConvertToUrl(commitEvent)
                    .Select(e =>
                        messages.AddAsync(new DownloadPngMessage()
                        {
                            Url = e,
                            Name = System.IO.Path.GetFileName(new Uri(e).LocalPath),
                            CorrelationId = correlationId
                        }, cancellationToken));

                await Task.WhenAll(tasks);
            }
        }

        [FunctionName(nameof(DownloadPictures))]

        public async Task DownloadPictures(
            [QueueTrigger(DownloadPng, Connection = "AzureWebJobsStorage")]
            DownloadPngMessage message,
            IBinder binder,
            [Table(PetsTable, Connection = "AzureWebJobsStorage")]
            IAsyncCollector<PetEntity> tables,
            ILogger log)
        {
            var correlationId = message.CorrelationId;
            using (log.BeginScope(new Dictionary<string, object>() {[nameof(correlationId)] = correlationId}))
            {
                log.LogInformation("Downloading image {name}", message.Name);
                var stream = await _pngService.GetPngAsync(message.Url);
                log.LogInformation("downloaded png");

                var blobName = $"pets/{message.Name}.png";
                var attribute = new BlobAttribute(blobName, FileAccess.Write)
                {
                    Connection = "AzureWebJobsStorage"
                };

                await using var blob = await binder.BindAsync<Stream>(attribute);
                await stream.CopyToAsync(blob);
                await tables.AddAsync(new PetEntity()
                {
                    BlobUrl = $"https://{GetAccountName()}.blob.core.windows.net/{blobName}",
                    PartitionKey = DateTime.UtcNow.Date.ToString("yy-MM-dd"),
                    RowKey = message.Name
                });

                await tables.FlushAsync();
                log.LogInformation("Saved Blob and reference {name}", message.Name);
            }
        }

        [FunctionName(nameof(TodayPng))]
        public async Task<IActionResult> TodayPng(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]  HttpRequest req,
            [Table(PetsTable , Connection = "AzureWebJobsStorage")]CloudTable  cloudTable)
        {
            TableQuerySegment<PetEntity> segment = null;
            var results = new List<PetEntity>();
            while (segment == null || segment.ContinuationToken != null)
            {
                var query =
                    new TableQuery<PetEntity>()
                        .Where(TableQuery.GenerateFilterCondition(
                            nameof(TableEntity.PartitionKey),
                            QueryComparisons.Equal,
                            DateTime.UtcNow.ToString("yy-MM-dd")));
                
                segment = await cloudTable.ExecuteQuerySegmentedAsync(query, segment?.ContinuationToken);
                results.AddRange(segment.Results.AsEnumerable());
            }

            return new OkObjectResult(results.Select(r => new
            {
                Name = r.RowKey,
                Day = r.PartitionKey,
                Url = r.BlobUrl
            }));
        }

        private string GetAccountName()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            var accountName = connectionString.Split(";").Where(s => s.StartsWith("AccountName="))
                .Select(e => e.Replace("AccountName=", string.Empty));
            return accountName.Single();
        }
    }
}