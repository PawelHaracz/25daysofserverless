using System.Net;
using System.Net.Http;
using Day8.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Day8
{
    public class SantaService
    {
        [FunctionName(nameof(HealthCheck))]
        public HttpResponseMessage HealthCheck(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]  HttpRequestMessage req,
            [Table("MyTable", "SantaService","1",  Connection = "AzureWebJobsStorage")] ServiceConfigurationTableEntity serviceConfigurationTableEntity,
            ILogger log)
        {
            return req.CreateResponse(serviceConfigurationTableEntity?.IsLive == true ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        }
    }
}