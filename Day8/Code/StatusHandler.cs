using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Day8
{
    public class StatusHandler
    {
        
//       [FunctionName(nameof(HealthCheckTimer))]
//        public Task HealthCheckTimer(
//            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, 
//            [DurableClient] IDurableOrchestrationClient starter,
//            ILogger log)
//        {
//            return starter.StartNewAsync(nameof(Run));
//        }
//
//        [FunctionName(nameof(Run))]
//        public Task Run(
//            [OrchestrationTrigger] IDurableOrchestrationContext context)
//        {
//            return  Task.CompletedTask;;
//        }
        
    }
}