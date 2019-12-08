using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Day8.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day8
{
    public class ServiceApi
    {
        [FunctionName(nameof(AddService))]
        public async Task<HttpResponseMessage> AddService(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Service/{entityKey}")]
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
            return req.CreateResponse(HttpStatusCode.Created);
        }

        [FunctionName(nameof(Get))]
        public async Task<HttpResponseMessage> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Service/{entityKey}")]
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
            else
            {
                return req.CreateResponse(HttpStatusCode.NotFound);   
            }
        }
    }
}