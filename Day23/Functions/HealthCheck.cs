using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Day23
{
    public static class HealthCheck
    {
        [FunctionName(nameof(HealthCheck))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var isError = Environment.GetEnvironmentVariable("isError", EnvironmentVariableTarget.Process);
            return string.IsNullOrWhiteSpace(isError)
                ? (ActionResult)new OkResult()
                : new BadRequestResult();
        }
    }
}
