using System.Collections.Generic;
using Day3.Model;

namespace Day3.Services
{
    public interface IPngConverter
    {
        IEnumerable<string> ConvertToUrl(GitCommitEvent @event);
    }
}