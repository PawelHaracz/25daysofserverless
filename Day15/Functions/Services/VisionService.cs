using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Day15.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Day15.Services
{
    public class VisionService
    {
        private readonly ComputerVisionClient _client;

        public VisionService(ComputerVisionClient client)
        {
            _client = client;
        }

        public async Task<PictureDescriptionModel> Describe(SearchedImage image)
        {
            var features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Color, VisualFeatureTypes.Objects
            };
            await using var ms = new MemoryStream(image.Bytes);
            var results = await _client.AnalyzeImageInStreamAsync(ms, features);
            var description = results.Description.Captions.Select(c => c.Text);
            var categories = results.Categories.Select(c => c.Name);
            var objects = results.Objects.Select(o => o.ObjectProperty);
            
            var colors = new Dictionary<string, string>()
            {
                [nameof(results.Color.AccentColor)] =results.Color.AccentColor,
                [nameof(results.Color.DominantColorBackground)] = results.Color.DominantColorBackground,
                [nameof(results.Color.DominantColorForeground)] = results.Color.DominantColorForeground,
            };
            return new PictureDescriptionModel(description, categories, colors, objects);
        }
    }
}
