using System;
using System.Net.Http;
using System.Threading.Tasks;
using Day7;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Day7
{
    public class SeventhDay
    {
        private readonly PictureService _pictureService;

        public SeventhDay(PictureService pictureService)
        {
            _pictureService = pictureService;
        }
        
        [FunctionName(nameof(Search))]
        public async Task<IActionResult> Search(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log
            )
        {
            try
            {
                string search = req.Query["s"];
                
                var picture = await _pictureService.GetPicture(search);
                
                var file = new FileContentResult(picture.Bytes, new MediaTypeHeaderValue("application/octet-stream"))
                {
                    FileDownloadName = picture.Name
                };
                return file;
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}