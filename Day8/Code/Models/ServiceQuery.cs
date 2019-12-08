namespace Day8.Models
{
    public class ServiceQuery
    {
        public ServiceQuery(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }
        public string Url { get; }
    }
}