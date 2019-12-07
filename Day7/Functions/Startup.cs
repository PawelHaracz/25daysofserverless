using Day7.Options;
using Day7;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day7
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddOptions<ImageOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("bing").Bind(options));
            builder.Services.AddScoped(c =>
            {
                var options = c.GetService<IOptions<ImageOptions>>();
                return new ImageSearchAPI(new ApiKeyServiceClientCredentials(options.Value.SubscriptionKey));
            });
            builder.Services.AddScoped<PictureService>();


        }
    }
}