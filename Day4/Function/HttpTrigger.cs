using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Day4.Models.CosmosModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day4
{
    public class FourthDay
    {
        [FunctionName(nameof(GetList))]
        public IActionResult GetList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "parties/{organizer}")]
            HttpRequest req,
            [CosmosDB(
            databaseName: "Parties",
                collectionName: "Parties",
                PartitionKey = "{organizer}",
                SqlQuery =  "select p.id, p.name, p.description from Parties p",
                ConnectionStringSetting = "CosmosDBConnection")] IEnumerable<PartyModel> parties,    
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(GetList));
               
                return new OkObjectResult(parties.Select(p => new
                {
                    p.Description,
                    p.Id,
                    p.Name
                }));
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
        
        [FunctionName(nameof(Get))]
        public IActionResult Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "parties/{organizer}/{id}")]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Parties",
                collectionName: "Parties",
                PartitionKey = "{organizer}",
                Id = "{id}",
                ConnectionStringSetting = "CosmosDBConnection")] PartyModel partyModel,    
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(Get));
               
                return new OkObjectResult(partyModel);
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
        
        [FunctionName(nameof(Post))]
        public async Task<IActionResult> Post(
            [HttpTrigger (AuthorizationLevel.Anonymous, "post", Route = "parties/{organizer}/{id}")]
            HttpRequest req,
            string organizer,
            string id,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,    
            [CosmosDB(
                databaseName: "Parties",
                collectionName: "Parties",
                PartitionKey = "{organizer}",
                Id = "{id}",
                ConnectionStringSetting = "CosmosDBConnection")] PartyModel partyModel,    
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(Post));

                using (var streamReader = new StreamReader(req.Body))
                {
                    var requestBody = await streamReader.ReadToEndAsync();
                    var model = JsonConvert.DeserializeObject<Food>(requestBody);

                    if (partyModel.FoodList.Any(c => c.Name == model.Name && string.Equals(c.Owner, model.Name, StringComparison.InvariantCulture)))
                    {
                        return new OkObjectResult(partyModel);
                    }

                    partyModel.FoodList.Add(model);
                    var collectionUri = UriFactory.CreateDocumentUri("Parties", "Parties", id);
                    var response = await client.ReplaceDocumentAsync(
                        collectionUri,
                        partyModel,
                        new RequestOptions()
                        {
                            PartitionKey = new PartitionKey(organizer),
                            JsonSerializerSettings = new JsonSerializerSettings()
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }
                        });

                    return new OkObjectResult(partyModel);
                }
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
        
        [FunctionName(nameof(Delete))]
        public async Task<IActionResult> Delete(
            [HttpTrigger (AuthorizationLevel.Anonymous, "delete", Route = "parties/{organizer}/{id}/{name}")]
            HttpRequest req,
            string organizer,
            string id,
            string name,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,    
            [CosmosDB(
                databaseName: "Parties",
                collectionName: "Parties",
                PartitionKey = "{organizer}",
                Id = "{id}",
                ConnectionStringSetting = "CosmosDBConnection")] PartyModel partyModel,    
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(Delete));
                
                var toDelete = partyModel.FoodList.FirstOrDefault(c => c.Name == name);
                if (toDelete is null)
                {
                    return new OkObjectResult(partyModel); 
                }
                
                partyModel.FoodList.Remove(toDelete);
                var collectionUri = UriFactory.CreateDocumentUri("Parties", "Parties", id);
                var response = await client.ReplaceDocumentAsync(
                    collectionUri, 
                    partyModel, 
                    new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(organizer),
                        JsonSerializerSettings = new JsonSerializerSettings()
                        {
                            
                        }
                    });

                return new OkObjectResult(partyModel);
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}