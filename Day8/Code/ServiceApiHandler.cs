using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Day8.Entities;
using Day8.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Day8
{
    public class ServiceApi
    {
        [FunctionName(nameof(AddService))]
        public async Task<HttpResponseMessage> AddService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Service/{entityKey}")]
            HttpRequestMessage req,
            [DurableClient] IDurableEntityClient entityClient,
            string entityKey,
            ILogger log)
        {
            var command = await req.Content.ReadAsAsync<CreateServiceCommand>();
            if (command == null) throw new ArgumentNullException(nameof(command));

            var entityId = new EntityId(nameof(ServiceAggregator), entityKey);
            
            var state = await entityClient.ReadEntityStateAsync<ServiceQuery>(entityId);
            if (state.EntityExists)
            {
                return req.CreateResponse(HttpStatusCode.Conflict);
            }
            
            await entityClient.SignalEntityAsync<IServiceAggregator>(entityId, proxy => proxy.Create(command));
            await entityClient.SignalEntityAsync<IEntitiesAggregator>(new EntityId(nameof(EntitiesAggregator), EntitiesAggregator.EntityId),
                proxy => proxy.Add(entityKey));
            return req.CreateResponse(HttpStatusCode.Created);
        }

        [FunctionName(nameof(Get))]
        public async Task<HttpResponseMessage> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Service/{entityKey}")]
            HttpRequestMessage req,
            [DurableClient] IDurableEntityClient entityClient,
            string entityKey,
            ILogger log)
        {
            var entityId = new EntityId(nameof(ServiceAggregator), entityKey);
            var state = await entityClient.ReadEntityStateAsync<ServiceQuery>(entityId);
            if (state.EntityExists)
            {
                var json = JsonConvert.SerializeObject(state.EntityState,
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                
                return new HttpResponseMessage(HttpStatusCode.OK) 
                {
                    Content = new StringContent(json, Encoding.UTF8, JsonMediaTypeFormatter.DefaultMediaType.ToString())
                };
            }

            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        
        [FunctionName(nameof(Investigate))]
        public async Task<HttpResponseMessage> Investigate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Service/{entityKey}/{status:int}")]
            HttpRequestMessage req,
            [DurableClient] IDurableEntityClient entityClient,
            string entityKey,
            int status,
            ILogger log)
        {
            
            var entityId = new EntityId(nameof(ServiceAggregator), entityKey);

            var stat = status == 0 ? Status.Closed : Status.Ongoing;
            await entityClient.SignalEntityAsync<IServiceAggregator>(entityId, proxy => proxy.ChangeState(new StatusCommand(stat)));
            await entityClient.SignalEntityAsync<IEntitiesAggregator>(new EntityId(nameof(EntitiesAggregator), EntitiesAggregator.EntityId),
                proxy => proxy.Add(entityKey));
            return req.CreateResponse(HttpStatusCode.Created);
        }
        
        
        [FunctionName(nameof(GetAllEntities))]
        public async Task<HttpResponseMessage> GetAllEntities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "Service/")]
            HttpRequestMessage req,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            
            var entityId = new EntityId(nameof(EntitiesAggregator), EntitiesAggregator.EntityId);

            var entityStateAsync = await entityClient.ReadEntityStateAsync<JObject>(entityId);
            if (entityStateAsync.EntityExists)
            {
                if (entityStateAsync.EntityState.HasValues is true)
                {
                    var obj = entityStateAsync.EntityState.GetValue("entities").ToObject<IEnumerable<Service>>();
                    if (obj is null)
                    {
                        return req.CreateResponse(HttpStatusCode.NoContent);
                    }

                    var json = JsonConvert.SerializeObject(obj.Select(e => e.Id),
                        new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8,
                            JsonMediaTypeFormatter.DefaultMediaType.ToString())
                    };
                }
            }

            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        
    }
}