using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Day19.Entities;
using Day19.Entities.Interfaces;
using Day19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Day19.Handlers
{
    public class IoTHubHandlers
    {
        [FunctionName(nameof(ReceiveMessages))]
        public async Task ReceiveMessages(
            [EventHubTrigger("%EventHubName%", ConsumerGroup = "%EventHubConsumerGroup%", Connection  = "EventHubConnectionAppSetting")] EventData[] eventData,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            var data = eventData.Select(e =>
                    Encoding.UTF8.GetString(e.Body))
                .Select(JsonConvert.DeserializeObject<DeviceTelemetry>)
                .GroupBy(d => d.DeviceId)
                .Select(kv => new
                {
                    Id = kv.Key,
                    BallonPresure = kv.Max(k => k.Pressure),
                    CompressorPressure = kv.Min(k => k.CompressorPressure)
                });

            foreach (var telemetryGroup in data)
            {
                var entityId = new EntityId(nameof(CompressorEntity), telemetryGroup.Id);
                
                await entityClient.SignalEntityAsync<ICompressorEntity>(entityId, async proxy =>
                {
                    await proxy.UpdatePressure(telemetryGroup.CompressorPressure);
                    await proxy.Calculate(telemetryGroup.BallonPresure);
                });
            }
        }

        [FunctionName(nameof(ReadStatus))]
        public async Task<IActionResult>  ReadStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Compressor/{deviceId}")] HttpRequestMessage req,
            string deviceId,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            var entityId = new EntityId(nameof(CompressorEntity), deviceId);

            var state = await entityClient.ReadEntityStateAsync<CompressorEntity>(entityId);

            if (state.EntityExists)
            {
                var model = new
                {
                    state.EntityState.Balloons,
                    state.EntityState.Pressure
                };
                return new OkObjectResult(model); 
            }
            
            return new NotFoundResult();
        }
    }
}