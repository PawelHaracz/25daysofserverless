using System.Linq;
using Day1;
using Day1.Options;
using Day1.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day1
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<DreidelOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                var section = configuration.GetSection("Dreidel");
                if (section.Exists() == false)
                {
                    options.Dreidels = new[] {"נ", "ג", "ה", "ש"}.AsEnumerable();
                }
                else
                {
                    options.Dreidels = section.AsEnumerable().Select(kv => kv.Value).AsEnumerable();
                }
            });

            builder.Services.AddSingleton<IDeridelGenerator, DeridelGenerator>();
        }
    }
}