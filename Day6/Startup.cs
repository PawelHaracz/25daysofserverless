using Day6;
using Day6.Options;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Day6
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<LuisService>();
            builder.Services.AddOptions<SlackOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("Slack").Bind(options));
            builder.Services.AddOptions<LuisOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("Luis").Bind(options));
        }
    }
}