namespace Day9.Models
{
    public class Event
    {
        public string Action { get; set; }
        public Issue Issue { get; set; }
        
        public RepositoryGitHub Repository { get; set; }
    }
    
}