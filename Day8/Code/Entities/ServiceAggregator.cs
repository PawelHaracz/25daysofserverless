using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Day8.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;

namespace Day8.Entities
{
    public interface IServiceAggregator 
    {
        Task Create(CreateServiceCommand command);
     
        Task ChangeState(StatusCommand status);
        Task<ServiceQuery> Get();
    }
    
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceAggregator : IServiceAggregator
    {
        [Newtonsoft.Json.JsonIgnore]
        private readonly IAsyncCollector<SignalRMessage> _asyncCollector;

        [JsonProperty("name")] 
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        private IDictionary<Status, Status> _statusMatrix = new Dictionary<Status, Status>()
        {
            [Status.Open] = Status.Ongoing,
            [Status.Ongoing] = Status.Closed,
            [Status.Closed] = Status.Open
        };

        public ServiceAggregator(IAsyncCollector<SignalRMessage> asyncCollector)
        {
            _asyncCollector = asyncCollector;
        }
        
        public Task Create(CreateServiceCommand command)
        {
            Name = command.Name;
            Url = command.Url;
            Status = Status.Closed;
            return Task.CompletedTask;
        }

        public Task ChangeState(StatusCommand status)
        {
            if (_statusMatrix[this.Status] == status.Status)
            {
                Status = status.Status;
            }
            
            return _asyncCollector.AddAsync(new SignalRMessage()
            {
                Target = "newStatus",
                Arguments = new[] { new
                {
                    Name = Entity.Current.EntityKey,
                    Status = (int)this.Status
                } }
            });
        }
        
        public Task<ServiceQuery> Get()
        {
            return Task.FromResult(new ServiceQuery(Name, Url, Status.ToString()));
        }

        [FunctionName(nameof(ServiceAggregator))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            [SignalR(HubName = "statusChanged")] IAsyncCollector<SignalRMessage> signalRMessages
            )
        {
            return ctx.DispatchAsync<ServiceAggregator>(signalRMessages);
        }

        
    }
}