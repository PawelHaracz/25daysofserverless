using Day25;
using Day25.Speech.Options;
using Day25.Speech.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day25
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            
            builder.Services.AddOptions<ComputeSpeechOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("compute").Bind(options));
            builder.Services.AddScoped(c =>
            {
                var options = c.GetService<IOptions<ComputeSpeechOptions>>();
                return SpeechConfig.FromSubscription(options.Value.AccessKey, options.Value.Region);
            });
            builder.Services.AddScoped<SpeechService>();
        }
    }
}