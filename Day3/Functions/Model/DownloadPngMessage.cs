namespace Day3.Model
{
    public class DownloadPngMessage
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string CorrelationId { get; set; }
    }
}