using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Day16.Entities;
using Day16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Day16.Handlers
{
    public class RegistrationHandler
    {
        private JsonSerializerSettings _settings;

        public RegistrationHandler()
        {
            _settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>()
                {
                    new StringEnumConverter(),
                },
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateFormatString = "dd-MM-yyyy HH:mm:ss",
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
        }

        [FunctionName(nameof(RunOrchestrator))]
        public  async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallEntityAsync(new EntityId(nameof(RegistrationAggregate), context.InstanceId),
                nameof(RegistrationAggregate.Create), context.CurrentUtcDateTime);
            var finishRegistration = await context.WaitForExternalEvent(nameof(FinishRegistration),  TimeSpan.FromMinutes(5), Status.TimeOut);

            await context.CallEntityAsync(new EntityId(nameof(RegistrationAggregate), context.InstanceId),
                nameof(RegistrationAggregate.SetState), new UpdateStatusCommand()
                {
                    Status = finishRegistration,
                    Time = context.CurrentUtcDateTime
                });
        }

        [FunctionName(nameof(FinishRegistration))]
        public async Task<IActionResult> FinishRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration/{instanceId}")]
            HttpRequestMessage req,
            string instanceId,
            [DurableClient] IDurableOrchestrationClient client,
            [DurableClient] IDurableEntityClient entityClient)
        {
            var entity = await entityClient.ReadEntityStateAsync<RegistrationAggregate>(new EntityId(nameof(RegistrationAggregate),
                    instanceId));
            
            if (entity.EntityExists is false)
            {
                return new NotFoundResult();
            }

            if (entity.EntityState.Status is Status.TimeOut)
            {
                return new ConflictResult();
            } 
            
            await client.RaiseEventAsync(instanceId, nameof(FinishRegistration), Status.Finished);
            return new OkResult();
        }
        

        [FunctionName(nameof(StartRegistration))]
        public async Task<HttpResponseMessage> StartRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(nameof(RunOrchestrator), null);
            
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(instanceId)
            };
        }
        
        [FunctionName(nameof(GetAll))]
        public async Task<IActionResult> GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registration")]HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var state = await client.ReadEntityStateAsync<AllRegistrationAggregate>(
                new EntityId(nameof(AllRegistrationAggregate), AllRegistrationAggregate.EntityId));

            if (state.EntityExists is false || state.EntityState.EntityIds.Any() is false)
            {
                return new NoContentResult();
            }

            var enumerable = state.EntityState.EntityIds.Select(id =>
                client.ReadEntityStateAsync<RegistrationAggregate>(new EntityId(nameof(RegistrationAggregate), id)));

            var records = await Task.WhenAll(enumerable);
            
            return new OkObjectResult(
                JsonConvert.SerializeObject(
                    records.Where(r 
                            => r.EntityExists)
                            .Select(r => r.EntityState), _settings));
        }
        
        
        [FunctionName(nameof(Get))]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registration/{instanceId}")]HttpRequestMessage req,
            string instanceId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return new BadRequestResult();
            }
            
            var state = await client.ReadEntityStateAsync<RegistrationAggregate>(
                new EntityId(nameof(RegistrationAggregate), instanceId));

            if (state.EntityExists is false)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(state.EntityState, _settings));

        }
    }
}