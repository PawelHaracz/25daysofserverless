using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Day16.Entities.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day16.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PosadasAllAggregatorAggregator : IPasadasAllAggregator
    {
        [JsonIgnore]
        public const string EntityId = "All";
        
        [JsonProperty(nameof(EntityIds))]
        public IList<string> EntityIds { get; set; }

        public PosadasAllAggregatorAggregator()
        {
            EntityIds = new List<string>();
        }
        
        [FunctionName(nameof(PosadasAllAggregatorAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<PosadasAllAggregatorAggregator>();
        }

        public Task Add(string id)
        {
            if (EntityIds.Contains(id) is false)
            {
               EntityIds.Add(id);
            }
            
            return Task.CompletedTask;
        }
    }
}