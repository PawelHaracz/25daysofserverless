using System;

namespace Day16.Models
{
    public class UpdateStatusCommand
    {
        public DateTime Time { get; set; }
        public Status Status { get; set; }
    }
}