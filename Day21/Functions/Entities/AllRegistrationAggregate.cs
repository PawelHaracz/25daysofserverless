using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day16.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AllRegistrationAggregate 
    {
        [JsonIgnore]
        public const string EntityId = "All";
        
        [JsonProperty(nameof(EntityIds))]
        public HashSet<string> EntityIds { get; }  = new HashSet<string>();

        [FunctionName(nameof(AllRegistrationAggregate))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<AllRegistrationAggregate>();
        }

        public Task Register(string id)
        {
            if (EntityIds.Contains(id) is false)
            {
                EntityIds.Add(id);
            }

            return Task.CompletedTask;
        }
    }
}