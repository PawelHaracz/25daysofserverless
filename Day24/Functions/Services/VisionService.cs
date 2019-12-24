using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Day24.Services
{
    public class VisionService
    {
        private readonly ComputerVisionClient _client;

        public VisionService(ComputerVisionClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<string>> GetTags(Stream image)
        {
            var features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Tags
            };
            var results = await _client.AnalyzeImageInStreamAsync(image, features);
            var tags = results.Tags.Select(t => t.Name);

            return tags;
        }
    }
}
