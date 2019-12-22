using System;
using System.Collections.Generic;
using System.Linq;
using Day22;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Day22
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddOptions<KeyVaultOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                var section = configuration.GetSection("keyVault");
                options.KeyVaultName = section.GetValue<string>("name") ?? throw new ArgumentNullException("keyVault__name");
                var secretSection = section.GetSection("secrets") ?? default;
                var list = new List<string>();
                foreach (var keyValuePair in secretSection.AsEnumerable())
                {
                    if (string.IsNullOrWhiteSpace(keyValuePair.Value))
                    {
                        continue;
                    }
                    list.Add(keyValuePair.Value);
                }

                options.Secrets = list.AsEnumerable();
            });

        }
    }
}