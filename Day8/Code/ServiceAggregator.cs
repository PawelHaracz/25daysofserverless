using System.Threading.Tasks;
using Day8.Models;
using DurableTask.Core.Stats;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Day8
{
    public interface IServiceAggregator
    {
        Task Create(CreateServiceCommand command);
        Task ChangeState(StatusCommand status);
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceAggregator : IServiceAggregator
    {
        [JsonProperty("name")] 
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        public Task Create(CreateServiceCommand command)
        {
            if (HasCreated())
            {
                return Task.CompletedTask;
            }
            
            Name = command.Name;
            Url = command.Url;
            Status = Status.Closed;
            return Task.CompletedTask;
        }

        public Task ChangeState(StatusCommand status)
        {
            Status = status.Status;
            return Task.CompletedTask;
        }

        private bool HasCreated()
        {
            return string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Url) == true;
        }
        
        [FunctionName(nameof(ServiceAggregator))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<ServiceAggregator>();
        }
    }
}