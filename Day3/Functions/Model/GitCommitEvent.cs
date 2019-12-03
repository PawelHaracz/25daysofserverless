using System.Collections;
using System.Collections.Generic;

namespace Day3.Model
{
    public class GitCommitEvent
    {
        public IEnumerable<Commit> Commits {get; set;}
        public Repository Repository { get; set; }
    }
}