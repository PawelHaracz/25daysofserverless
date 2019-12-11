using Day11;
using Day11.Options;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day11
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<SlackService>();
            builder.Services.AddOptions<SlackOptions>()
                .Configure<IConfiguration>(
                    (options, configuration) => configuration.
                        GetSection("Slack")
                        .Bind(options));
        }
    }
}