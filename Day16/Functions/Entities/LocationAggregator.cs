using System.Threading.Tasks;
using Day16.Entities.Interfaces;
using Day16.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day16.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LocationAggregator : ILocationAggregator
    {
        private readonly IDurableEntityContext _context;

        public LocationAggregator(IDurableEntityContext context)
        {
            _context = context;
        }
        
        [JsonProperty(nameof(Name))]
        public string Name { get; set; }
        
        [JsonProperty(nameof(Latitude))]
        public double Latitude { get; set; }
        
        [JsonProperty(nameof(Longitude))]
        public double Longitude { get; set; }
        
        [FunctionName(nameof(LocationAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<LocationAggregator>(ctx);
        }

        public Task Add(AddLocationCommand command)
        {
            if (command is null)
            {
                _context.DeleteState();
            }
            Name = command.Name;
            Longitude = command.Longitude;
            Latitude = command.Latitude;
            return  Task.CompletedTask;
        }
    }
}