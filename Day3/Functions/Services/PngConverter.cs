using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Day3.Model;
using Microsoft.Extensions.Logging;

namespace Day3.Services
{
    public class PngConverter: IPngConverter
    {
        private readonly ILogger<PngConverter> _logger;

        public PngConverter(ILogger<PngConverter> logger)
        {
            _logger = logger;
        }
        
        public IEnumerable<string> ConvertToUrl(GitCommitEvent commitEvent)
        {
            if (string.IsNullOrWhiteSpace(commitEvent?.Repository?.HtmlUrl))
            {
                _logger.LogInformation("commit {commitId} event doesn't have html url", commitEvent?.Repository?.Id);
            }

            var pngObjects = commitEvent?.Commits.SelectMany(c => c.Added
                ?.Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .Select(e => $"{commitEvent.Repository.HtmlUrl}/blob/{c.Id}/{e}"))
                .ToList();

            if (pngObjects is null)
            {
                _logger.LogInformation("PngObjects object is null");
                return ImmutableArray<string>.Empty;
            }
            if (pngObjects.Any() is false)
            {
                _logger.LogInformation("Any Commits doesn't have any png");
                return ImmutableArray<string>.Empty;
            }

            return pngObjects.AsEnumerable();
        }
    }
}