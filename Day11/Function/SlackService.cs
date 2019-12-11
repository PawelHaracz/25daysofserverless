using System;
using System.Net.Http;
using System.Threading.Tasks;
using Day11.Models.CosmosModels;
using Day11.Models.Rest;
using Day11.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day11
{
    public class SlackService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SlackOptions _options;
        private readonly ILogger _logger;

        public SlackService(IHttpClientFactory httpClientFactory,IOptions<SlackOptions> options, ILogger<SlackService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.Url))
            {
                throw new ArgumentNullException(nameof(_options.Url));
            }
        }

        public async Task Notify(WishModel model)
        {
            _logger.LogInformation("Processing gift : {giftId}", model.Id);
            var message = $"new wish just arrived, please - {model.Who} wants {model.Type} - {model.Description}. Gift needs to be shipped to {model.Address}";
            _logger.LogInformation("Sending gift : {giftId} - {message}", model.Id, message);
            using var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_options.Url),
                Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new SlackModel
                        {
                            Text = message
                        },
                        new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }))
            });

            response.EnsureSuccessStatusCode();
        }
    }
}