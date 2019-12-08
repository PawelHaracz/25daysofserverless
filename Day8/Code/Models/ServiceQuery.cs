namespace Day8.Models
{
    public class ServiceQuery
    {
        public ServiceQuery(string name, string url, string status)
        {
            Name = name;
            Url = url;
            Status = status;
        }

        public string Name { get; }
        public string Url { get; }
        public string Status { get; }
    }
}