using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Day20.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Day20
{
    public class Day20
    {
        private readonly VisionService _visionService;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private readonly IReadOnlyCollection<string> _allowContentType = new []
        {
            "image/png",
            "image/jpg",
            "image/jpeg"
        };

        private readonly IReadOnlyCollection<string> _giftValidator = new[]
        {
            "box",
            "gift wrapping",
            "ribbon",
            "present"
        };
        
        public Day20(VisionService visionService)
        {
            _visionService = visionService;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        
        [FunctionName(nameof(UploadToBlob))]
        public async Task<IActionResult> UploadToBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Gift")]
            HttpRequest req,
            IBinder binder,
            CancellationToken cancellationToken)
        {
            using var streamContent = new StreamContent(req.Body)
            {
                Headers =
                {
                    ContentType = MediaTypeWithQualityHeaderValue.Parse( req.ContentType)
                }
            };
            var content = await streamContent.ReadAsMultipartAsync(cancellationToken);

            var allowedContents =
                content.Contents.Where(hc => 
                    _allowContentType.Contains(hc.Headers.ContentType.MediaType.ToLower())).ToArray();

            if (allowedContents.Any() == false)
            {
                return new BadRequestResult();
            }
            
            async Task UploadBlob(HttpContent httpContent1)
            {
                var blobName = $"gifts/{Guid.NewGuid():N}.png";
                var attribute = new BlobAttribute(blobName, FileAccess.Write)
                {
                    Connection = "AzureWebJobsStorage"
                };
                var stream = await httpContent1.ReadAsStreamAsync();
                await using var blob = await binder.BindAsync<Stream>(attribute, cancellationToken);
                await stream.CopyToAsync(blob, cancellationToken);
            }

            var tasks = allowedContents.Select(UploadBlob);
            await Task.WhenAll(tasks);
            
            return new AcceptedResult();
        }
        
         [FunctionName(nameof(Detect))]
        public async Task Detect(
            [BlobTrigger("gifts/{fileName}",Connection = "AzureWebJobsStorage")]Stream blob,
            string fileName,
            [SignalR(HubName = SignalRHandler.HubName)]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log
            )
        {
            try
            {
                var tags =await _visionService.GetTags(blob);

                var compared = _giftValidator.All(t => tags.Contains(t.ToLower()));
                var message = new SignalRMessage()
                {
                    Target = "broadcastMessage"
                };

                message.Arguments = new object[]
                {
                    new
                    {
                      FileName = fileName,
                      Result = compared
                    }
                };
                
                await signalRMessages.AddAsync(message);

            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}