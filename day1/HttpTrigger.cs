using System;
using Day1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Day1
{
    public class FirstDay
    {
        private readonly IDeridelGenerator _deridelGenerator;

        public FirstDay(IDeridelGenerator deridelGenerator)
        {
            _deridelGenerator = deridelGenerator;
        }
        
        [FunctionName(nameof(FirstDay))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Starting processing function {functionName}", nameof(FirstDay));
                var @return = _deridelGenerator.Get();
                log.LogInformation("Generated deridel {deridel}", @return);
                return new OkObjectResult(@return);
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}