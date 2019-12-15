using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Day15;
using Day15.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day15
{
    public class SeventhDay
    {
        private readonly PictureService _pictureService;
        private readonly VisionService _visionService;

        public SeventhDay(PictureService pictureService, VisionService visionService)
        {
            _pictureService = pictureService;
            _visionService = visionService;
        }
        
        [FunctionName(nameof(Search))]
        public async Task<HttpResponseMessage> Search(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log
            )
        {
            try
            {
                string search = req.Query["s"];
                if (string.IsNullOrEmpty((search)))
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }

                var picture = await _pictureService.GetPicture(search);
                var description = await _visionService.Describe(picture);

                var json = JsonConvert.SerializeObject(new
                    {
                        Description = description, Image = new
                        {
                            Base64 = Convert.ToBase64String(picture.Bytes),
                            Name = picture.Name
                        }
                    },
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}