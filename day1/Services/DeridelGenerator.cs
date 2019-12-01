using System;
using System.Linq;
using Day1.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Day1.Services
{
    public class DeridelGenerator : IDeridelGenerator
    {
        private readonly ILogger _logger;
        private static Random _random = new Random();
        private readonly DreidelOptions _deridels;

        public DeridelGenerator(IOptions<DreidelOptions> options, ILogger<DeridelGenerator> logger)
        {
            _logger = logger;
            _deridels = options.Value;
        }
        
        public string Get()
        {
            var index = _random.Next(_deridels.Dreidels.Count());
            _logger.LogInformation("Generated index : {index}", index.ToString());
            var value = _deridels.Dreidels.ElementAt(index);
            _logger.LogInformation("Deridel value : {value}", value);
            return value;
        }
    }
}