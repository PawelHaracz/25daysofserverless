using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Day25.Speech.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Day25
{
    public class SpeechHandler
    {
        private readonly SpeechService _speechService;

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private readonly IReadOnlyCollection<string> _allowContentType = new []
        {
            "audio/wav",
        };

        
        public SpeechHandler(SpeechService speechService)
        {
            _speechService = speechService;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        
        [FunctionName(nameof(UploadToBlob))]
        public async Task<IActionResult> UploadToBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "call")]
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

            var callId = Guid.NewGuid();
            async Task UploadBlob(HttpContent httpContent1)
            {
                var fileName = httpContent1.Headers.ContentDisposition.FileName?.Replace("\"", string.Empty);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var blobName = $"calls/{fileName}";
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
        
         [FunctionName(nameof(Transcript))]
       
        public async Task Transcript(
            [BlobTrigger("calls/{fileName}",Connection = "AzureWebJobsStorage")]Stream blob,
            string fileName,
            IBinder binder,
            [TwilioSms(From = "%fromPhoneNumber%", AccountSidSetting = "accountSid", AuthTokenSetting = "authToken")] IAsyncCollector<CreateMessageOptions> asyncCollector,
            ILogger log
            )
        {
            try
            {
                var text = await _speechService.ToText(blob);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }
                
                var blobName = $"transcripts/{fileName.Replace("wav", "txt")}";
                var attribute = new BlobAttribute(blobName, FileAccess.Write)
                {
                    Connection = "AzureWebJobsStorage"
                };
                
                await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                await using var blobTranscription = await binder.BindAsync<Stream>(attribute);
                await stream.CopyToAsync(blobTranscription);

                await asyncCollector.AddAsync(
                    new CreateMessageOptions(
                        new PhoneNumber(
                            Environment.GetEnvironmentVariable("toPhoneNumber", EnvironmentVariableTarget.Process)))
                    {
                        Body = "Your message has been saved and transcribed"
                    });
                log.LogInformation("Done");
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.Message);
                throw;
            }
        }
    }
}