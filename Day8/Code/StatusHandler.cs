using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Day8.Entities;
using Day8.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace Day8
{
    public class StatusHandler
    {
        
       [FunctionName(nameof(HealthCheckTimer))]
        public Task HealthCheckTimer(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, 
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            return starter.StartNewAsync(nameof(Run));
        }

        [FunctionName(nameof(Run))]
        public async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var proxy = context.CreateEntityProxy<IEntitiesAggregator>(new EntityId(nameof(EntitiesAggregator), EntitiesAggregator.EntityId));
            var entities = await proxy.GetWorkedServices();

            var tasks = new List<Task>();
            foreach (var entity in entities)
            {
                var entityId = new EntityId(nameof(ServiceAggregator), entity);
                var serviceProxy = context.CreateEntityProxy<IServiceAggregator>(entityId);
                var service = await serviceProxy.Get();
                
                tasks.Add(
                    context.CallSubOrchestratorAsync(nameof(HealCheckOrchestration),
                        new ServiceInput
                        {
                            Id = entity,
                            Name = service.Name,
                            Url = service.Url
                        }));
            }

            await Task.WhenAll(tasks);
        }
        
        [FunctionName(nameof(HealCheckOrchestration))]
        public async Task HealCheckOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var service = context.GetInput<ServiceInput>();
            var entityId = new EntityId(nameof(ServiceAggregator), service.Id);
            var serviceProxy = context.CreateEntityProxy<IServiceAggregator>(entityId);
            var httpClientResponse = await context.CallHttpAsync(HttpMethod.Get, new Uri(service.Url));

            var status = httpClientResponse.StatusCode == HttpStatusCode.OK ? Status.Closed : Status.Open;
                
            await serviceProxy.ChangeState(new StatusCommand(status));
        }

        internal class ServiceInput
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
        }
        
        
        
    }
}