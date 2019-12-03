using Day3;
using Day3.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(Startup))]
namespace Day3
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IPngConverter, PngConverter>();
            builder.Services.AddScoped<IPngService, PngService>();
        }
    }
}