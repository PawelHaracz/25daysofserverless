using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Day8.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EntitiesAggregator : IEntitiesAggregator
    {
        [JsonIgnore]
        public const string EntityId = "All";
        
        [JsonProperty("entities")]
        public IList<Service> Entities { get; set; }


        public Task Add(string entityKey)
        {
            if (Entities == null)
            {
                Entities = new List<Service>();
            }
            
            if (Entities.Any(e => e.Id == entityKey) == false)
            {
                Entities.Add(new Service()
                {
                    Id = entityKey,
                    IsActive = true
                });
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetWorkedServices()
        {
            if (Entities == null || Entities?.Count() == 0)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }
            return Task.FromResult(Entities.Where(e => e.IsActive).Select(e=>e.Id).AsEnumerable());
        }

        [FunctionName(nameof(EntitiesAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<EntitiesAggregator>();
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public struct Service
    {
        public string Id;
        public bool IsActive;
    }

    public interface IEntitiesAggregator
    {
        Task Add(string entityKey);
        Task<IEnumerable<string>> GetWorkedServices();
    }
}