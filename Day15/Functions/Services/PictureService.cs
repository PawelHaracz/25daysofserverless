using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Day15.Models;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;

namespace Day15.Services
{
    public class PictureService
    {
        private readonly ImageSearchAPI _imageSearchApi;
        private readonly IHttpClientFactory _clientFactory;

        public PictureService(ImageSearchAPI imageSearchApi, IHttpClientFactory clientFactory)
        {
            _imageSearchApi = imageSearchApi;
            _clientFactory = clientFactory;
        }

        public async Task<SearchedImage> GetPicture(string search)
        {
            var imageCollection = await _imageSearchApi.Images.SearchAsync(search);

            if (imageCollection.TotalEstimatedMatches.HasValue == false)
            {
                return new SearchedImage();
            }

            var image = imageCollection.Value.First();
            var url = image.ContentUrl;
            using var client = _clientFactory.CreateClient();
            var bytes = await client.GetByteArrayAsync(url);
            
            return new SearchedImage()
            {
                Name = image.ContentUrl.Split("/").Last(),
                Bytes = bytes
            };
        }
    }
}