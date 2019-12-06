using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Day6.Models;
using Day6.Options;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day6
{
    public class Scheduler
    {
        
        private readonly LuisService _luisService;
        private  readonly SlackOptions _options;
        
        public Scheduler(IOptions<SlackOptions> options, IHttpClientFactory clientFactory, LuisService luisService)
        {
            _luisService = luisService;
            _options = options.Value ?? throw new AggregateException(nameof(options));
        }
        
        [FunctionName(nameof(Scheduler))]
        public  async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext  context)
        {
            var message = context.GetInput<SlackMessage>();
            
            var model = await context.CallActivityAsync<LuisModel>((nameof(Scheduler) + nameof(GetWhen)), message.Text);
            var httpTask = context.CallHttpAsync(
                HttpMethod.Post, 
                new Uri(_options.WebHookUrl),
                JsonConvert.SerializeObject( new SlackMessage()
                {
                    Text = $"Scheduled task at {model.Time}"
                }, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));

            var timerTask = context.CreateTimer(model.Time, CancellationToken.None);

            await Task.WhenAll(httpTask,timerTask);
            
            await context.CallHttpAsync(
                HttpMethod.Post, 
                new Uri(_options.WebHookUrl),
                JsonConvert.SerializeObject( new SlackMessage()
                {
                    Text = $"Hey! time to do your task: {model.Task.ToUpper()}"
                }, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }
        
        [FunctionName(nameof(Scheduler)+nameof(GetWhen))]
        public async Task<LuisModel> GetWhen([ActivityTrigger] string message, ILogger log)
        {
            return await _luisService.DetectModel(message);
        }

        [FunctionName(nameof(Scheduler) + nameof(Start))]
        public async Task<HttpResponseMessage> Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient client,
            ILogger log)
        {
            var collection = await req.Content.ReadAsFormDataAsync();
            var eventData = new SlackMessage()
            {
                Text = collection.Get("text")
            };
            string instanceId = await client.StartNewAsync(nameof(Scheduler), eventData);
            
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

           return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}