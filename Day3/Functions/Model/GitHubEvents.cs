using System;

namespace Day3.Model
{
    [Flags]
    public enum GitHubEvents
    {
        Unknown = 0,
        Ping = 1,
        Push = 2 
    }
}