using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Day9
{
    public partial class HttpTrigger
    {
        private readonly IConfiguration _configuration;

        public HttpTrigger(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName(nameof(GitHubIssue))]
        public async Task<HttpResponseMessage> GitHubIssue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var @event = await req.Content.ReadAsAsync<Models.Event>();
            if (string.Equals(@event.Action, "opened", StringComparison.InvariantCulture) is false)
            {
                return req.CreateResponse(HttpStatusCode.OK);
            }

            var client = new GitHubClient(ProductHeaderValue.Parse("25-days-of-serverless"));
            var basicAuth = new Credentials(_configuration.GetValue<string>("GithubLogin"),
                _configuration.GetValue<string>("GithubPassword"));
            client.Credentials = basicAuth;
            var response = await client.Issue.Comment.Create(@event.Repository.Id, @event.Issue.Number,
                $@"@{@event.Issue.User.Login} Thank you for your Issue, - May the 25days of Serverless be with you");
            
            if (response is null)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}