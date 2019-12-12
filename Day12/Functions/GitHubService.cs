using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Day12.Options;
using Markdig;
using Microsoft.Extensions.Options;
using Octokit;

namespace Day12
{
    public class GitHubService
    {
        private readonly GitHubClient _client;

        public GitHubService(IOptions<GitHubOption> options)
        {
            _client = new GitHubClient(ProductHeaderValue.Parse("25-days-of-serverless"));
            var credentials = new Credentials(options.Value.Token);
            _client.Credentials = credentials;
        }

        public async Task<string> ConvertToHtml(string gistId)
        {
            var gist = await _client.Gist.Get(gistId);
            var files = gist.Files.Where(kv => kv.Key.EndsWith(".md"));
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<html>");
            stringBuilder.AppendLine("<head>");
            stringBuilder.AppendLine("</head>");
            stringBuilder.AppendLine("<body>");
            foreach (var file in files)
            {
                var result = Markdown.ToHtml(file.Value.Content);
                stringBuilder.AppendLine(result);
            }
            stringBuilder.AppendLine("</body>");
            stringBuilder.AppendLine("</html>");

            return stringBuilder.ToString();
        }
    }
}