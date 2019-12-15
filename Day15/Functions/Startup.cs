﻿using Day15.Options;
using Day15;
using Day15.Services;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day15
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            
            builder.Services.AddOptions<ImageOption>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("bing").Bind(options));
            builder.Services.AddScoped(c =>
            {
                var options = c.GetService<IOptions<ImageOption>>();
                return new ImageSearchAPI(
                    new Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials(
                        options.Value.SubscriptionKey));
            });
            builder.Services.AddScoped<PictureService>();    

            builder.Services.AddOptions<ComputeVisionOption>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("compute").Bind(options));
            builder.Services.AddScoped(c =>
            {
                var options = c.GetService<IOptions<ComputeVisionOption>>();
                return new ComputerVisionClient(
                    new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(
                        options.Value.AccessKey))
                {
                    Endpoint = options.Value.Endpoint
                };
            });    
            builder.Services.AddScoped<VisionService>();
        }
    }
}