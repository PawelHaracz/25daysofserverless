using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Day20
{
    public class SignalRHandler
    {
        public const string HubName = "ImageProcessing";
        [FunctionName("negotiate")]
        public IActionResult Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = null)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = HubName)]SignalRConnectionInfo info
        )
        {
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            
            return info != null
                ? (ActionResult)new OkObjectResult(info)
                : new NotFoundObjectResult("Failed to get SignalR connection information");
        }
    }
}