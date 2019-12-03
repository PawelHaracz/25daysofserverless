using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Day3.Services
{
    public class PngService: IPngService
    {
        private readonly HttpClient _client;
        private const string RawQuery = "?raw=true";
        
        public PngService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient();
        }
        
        public async Task<Stream> GetPngAsync(string pngUrl)
        {
            if (pngUrl.EndsWith(RawQuery) is false)
            {
                pngUrl = $"{pngUrl}{RawQuery}";
            }

            var response = await _client.GetAsync(pngUrl);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}