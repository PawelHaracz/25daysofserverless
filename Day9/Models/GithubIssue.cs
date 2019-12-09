namespace Day9.Models
{
    public class Issue
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public UserIssue User { get; set; }
    }
    
}