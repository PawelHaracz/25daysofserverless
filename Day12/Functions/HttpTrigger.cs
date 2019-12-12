using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Day12
{
    public class HttpTrigger
    {
        private readonly GitHubService _gitHubService;
        private readonly RedisService _redisService;

        public HttpTrigger(GitHubService gitHubService, RedisService redisService)
        {
            _gitHubService = gitHubService;
            _redisService = redisService;
        }

        [FunctionName(nameof(GistConverter))]
        public async Task<IActionResult> GistConverter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Convert/{gistId}")]
            HttpRequestMessage req,
            string gistId,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string html;
            var cache = await _redisService.Get(gistId);

            if (string.IsNullOrWhiteSpace(cache))
            {
                html = await _gitHubService.ConvertToHtml(gistId);
                await _redisService.Set(gistId, html);
            }
            else
            {
                html = cache;
            }
            
            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };;
        }
    }
}