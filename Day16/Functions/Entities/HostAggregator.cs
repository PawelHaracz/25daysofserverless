using System;
using System.Threading.Tasks;
using Day16.Entities.Interfaces;
using Day16.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day16.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HostAggregator : IHostAggregator
    {
        [JsonProperty(nameof(FirsName))]
        public string FirsName { get; set; }
        
        [JsonProperty(nameof(LastName))]
        public string LastName { get; set; }
        
        [FunctionName(nameof(HostAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<HostAggregator>();
        }

        public Task Create(CreateHostCommand createHostCommand)
        {
            FirsName = createHostCommand.FirstName;
            LastName = createHostCommand.LastName;
            return Task.CompletedTask;
        }
    }
}