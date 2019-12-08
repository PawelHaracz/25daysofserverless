using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Day8
{
    public class SignalRHandler
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = null)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "statusChanged")]SignalRConnectionInfo connectionInfo
        )
        {
            return connectionInfo;
        }
    }
}