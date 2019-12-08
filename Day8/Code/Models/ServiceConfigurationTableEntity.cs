using Microsoft.WindowsAzure.Storage.Table;

namespace Day8.Models
{
    public class ServiceConfigurationTableEntity : TableEntity
    {
        public bool IsLive { get; set; }
    }
}