using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Day6.Models;
using Day6.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Day6
{
    public class LuisService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<LuisService> _logger;
        private readonly LuisOptions _luisOptions;
        public LuisService(IHttpClientFactory clientFactory, IOptions<LuisOptions> options, ILogger<LuisService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _luisOptions = options.Value;
        }

        public async Task<LuisModel> DetectModel(string message)
        {
            using var httpClient = _clientFactory.CreateClient();
            
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString["subscription-key"] = _luisOptions.LuisSubscriptionKey;
            queryString["q"] = message;
            queryString["timezoneOffset"] = "0";
            
            var uri = _luisOptions.LuisUri + _luisOptions.LuisAppId + "?" + queryString;
            _logger.LogInformation("Calling the LUIS app to Get the Predections");
            var response = await httpClient.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            return DeserializeLuisModel(content);
        }

        private static LuisModel DeserializeLuisModel(string content)
        {
            var jObject = JObject.Parse(content);
            var entities = (JArray) jObject.SelectToken("entities");
            var dateTimeEntity =
                entities.FirstOrDefault(e => e.HasValues && e["type"].ToString() == "builtin.datetimeV2.datetime");

            if (DateTime.TryParse(dateTimeEntity?["resolution"]?["values"]?[0]["timex"].ToString(), out var dateTime) == false)
            {
                dateTime = DateTime.UtcNow;
            }

            return new LuisModel()
            {
                Task = jObject["query"].ToString().Replace(dateTimeEntity?["entity"].ToString() ?? string.Empty, string.Empty),
                Time = dateTime
            };
        }
    }
}