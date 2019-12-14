using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class SignalRHandler
    {
        private const string HubName = "chat";
        
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = null)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = HubName, UserId = "{headers.x-ms-client-principal-name}")]SignalRConnectionInfo connectionInfo 
        )
        {
            return connectionInfo;
        }
        
        [FunctionName(nameof(SendMessage))]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]Message message, 
            [SignalR(HubName = HubName)]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage 
                {
                    Target = "broadcastMessage",
                    UserId = message.recipient,
                    Arguments = new [] { new { message.sender, message.text}  }
                });
        }

        public class Message
        {
            public string recipient { get; set; }
            public string sender { get; set; }
            public string text { get; set; }
        }
    }
}
