using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Day11.Models.CosmosModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day11
{
    public class SantaHandler
    {
        private readonly SlackService _slackService;
        public SantaHandler(SlackService slackService)
        {
            _slackService = slackService;
        }
        
        [FunctionName(nameof(Post))]
        public async Task<IActionResult> Post(
            [HttpTrigger (AuthorizationLevel.Anonymous, "post", Route = "wish")]
            HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(Post));

                using var streamReader = new StreamReader(req.Body);
                var requestBody = await streamReader.ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<WishModel>(requestBody);
                model.Id = Guid.NewGuid();

                var collectionUri = UriFactory.CreateDocumentCollectionUri("Santa", "Wishes");
                var response = await client.CreateDocumentAsync(
                    collectionUri, 
                    model, 
                    new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(model.Type),
                        JsonSerializerSettings = new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }
                    });

                return new OkObjectResult(model);
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }

        [FunctionName(nameof(NotifyElves))]
        public async Task NotifyElves(
            [CosmosDBTrigger(
                databaseName: "Santa",
                collectionName: "Wishes",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true
                )] 
            IReadOnlyList<Document> wishes,
            ILogger log)
        {
            log.LogInformation("Receive new wishes");
            var tasks = wishes.Select(d => (WishModel)d).Select(_slackService.Notify);

            await Task.WhenAll(tasks);
        }

    }
} 