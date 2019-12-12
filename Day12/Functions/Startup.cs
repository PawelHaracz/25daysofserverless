using Day12;
using Day12.Options;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Day12
{
    
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<GitHubOption>()
                .Configure<IConfiguration>(
                    (option, configuration) => configuration.GetSection("GitHub").Bind(option));
            builder.Services.AddScoped<GitHubService>();
            
            builder.Services.AddOptions<RedisOption>()
                .Configure<IConfiguration>(
                    (option, configuration) => configuration.GetSection("Redis").Bind(option));
            builder.Services.AddScoped<RedisService>();
        }
    }
}