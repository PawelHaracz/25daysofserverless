using System.Threading.Tasks;
using Day16.Entities.Interfaces;
using Day16.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day16.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PosadasAggregator : IPasadasAggregator
    {
        [JsonIgnore]
        private readonly IDurableEntityContext _entityContext;
        

        public PosadasAggregator(IDurableEntityContext entityContext)
        {
            _entityContext = entityContext;
        }
        
        [JsonProperty(nameof(HostId))]
        public string HostId { get; set; }
        
        [JsonProperty(nameof(LocationId))]
        public string LocationId { get; set; }
        
        [FunctionName(nameof(PosadasAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<PosadasAggregator>(ctx);
        }

        public Task Create(CreatePasadasCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.HostId))
            {
                _entityContext.DeleteState();
                return Task.CompletedTask;
            }
            HostId = command.HostId;
            LocationId = command.LocationId;

            var entityId = new EntityId(nameof(PosadasAllAggregatorAggregator),
                PosadasAllAggregatorAggregator.EntityId);
            _entityContext.SignalEntity<IPasadasAllAggregator>(entityId, proxy =>
                proxy.Add(_entityContext.EntityKey));
            
            return Task.CompletedTask;
        }
    }
}