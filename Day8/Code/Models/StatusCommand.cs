namespace Day8.Models
{
    public class StatusCommand
    {
        public StatusCommand(Status status)
        {
            Status = status;
        }
        public Status Status { get; }
    }
}