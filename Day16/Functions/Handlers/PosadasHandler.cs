using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Day16.Entities;
using Day16.Entities.Interfaces;
using Day16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day16.Handlers
{
    public class PosadasHandler
    {
        [FunctionName(nameof(GetLocations))]
        public async Task<IActionResult> GetLocations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Locations")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log
        )
        {
            var entityId = new EntityId(nameof(PosadasAllAggregatorAggregator), PosadasAllAggregatorAggregator.EntityId);
            var aggregator = await entityClient.ReadEntityStateAsync<PosadasAllAggregatorAggregator>(entityId);
            if (aggregator.EntityExists is false)
            {
                return new NotFoundResult();
            }

            var @return = new List<LocationDetailQuery>();
            foreach (var id in aggregator.EntityState.EntityIds)
            {
                var query = await GetLocationDetailQuery(entityClient, id);
                if (query == default)
                {
                    continue;
                }
                @return.Add(query);
            }
            return new OkObjectResult(@return);
        }

        [FunctionName(nameof(PostLocation))]
        public async Task<IActionResult> PostLocation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Locations/{id}")]
            HttpRequestMessage req,
            string id,
            [DurableClient] IDurableEntityClient entityClient
        )
        {
            var command = await req.Content.ReadAsAsync<BeHostQuery>();
            
            var entityId = new EntityId(nameof(PosadasAggregator), id);
            var locationEntityId = new EntityId(nameof(LocationAggregator), command.LocationName);
            var hostEntityId = new EntityId(nameof(HostAggregator), command.NameId);

            var posades = await entityClient.ReadEntityStateAsync<PosadasAggregator>(entityId);
            if (posades.EntityExists)
            {
                return new ConflictResult();
            }
            
            var host = await entityClient.ReadEntityStateAsync<HostAggregator>(hostEntityId);
            if (host.EntityExists is false)
            {
                await entityClient.SignalEntityAsync<IHostAggregator>(hostEntityId,
                    proxy => proxy.Create(new CreateHostCommand("Pawel", "Haracz")));
            }

            await entityClient.SignalEntityAsync<ILocationAggregator>(locationEntityId, proxy => proxy.Add(new AddLocationCommand(command.LocationName, 1.4, 16)));

            await entityClient.SignalEntityAsync<IPasadasAggregator>(entityId,
                proxy => proxy.Create(new CreatePasadasCommand(command.NameId, command.LocationName)));
            
            return new CreatedResult("Locations", id);
        }
        
        private async ValueTask<LocationDetailQuery> GetLocationDetailQuery(IDurableEntityClient entityClient, string id)
        {
            var pasada =  await entityClient.ReadEntityStateAsync<PosadasAggregator>(new EntityId(nameof(PosadasAggregator), id));
            if (pasada.EntityExists is false)
            {
                return  default;
            }

            if (string.IsNullOrWhiteSpace(pasada.EntityState.HostId))
            {
                return default;
            }
            
            var hostTask = entityClient.ReadEntityStateAsync<HostAggregator>(
                new EntityId(nameof(HostAggregator), pasada.EntityState.HostId));
            var locationTask =  entityClient.ReadEntityStateAsync<LocationAggregator>(
                new EntityId(nameof(LocationAggregator), pasada.EntityState.LocationId));

            await Task.WhenAll(hostTask, locationTask);

            var hostResult = hostTask.Result;
            var locationResult = locationTask.Result;

            var hostName = hostResult.EntityExists
                ? $"{hostResult.EntityState.FirsName} {hostResult.EntityState.LastName}"
                : "unknown";

            var location = locationResult.EntityExists ? new Location(locationResult.EntityState.Name, locationResult.EntityState.Latitude, locationResult.EntityState.Longitude) : default;
                
                
            return new LocationDetailQuery(id, hostName, location);
        } 
    }
}