using System;
using System.Threading.Tasks;
using Day2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace Day2
{
    public class SecondDay
    {

        private const string HubName = "remainder";

        public SecondDay()
        {
        
        }
        
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = null)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = HubName)]SignalRConnectionInfo connectionInfo
            )
        {
            return connectionInfo;
        }
        
        [FunctionName(nameof(SendMessage))]
        public Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]object message, 
            [SignalR(HubName = HubName)]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage 
                {
                    Target = "broadcastMessage", 
                    Arguments = new [] { message } 
                });
        }
        
    }
}