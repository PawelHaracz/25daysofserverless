using Microsoft.WindowsAzure.Storage.Table;

namespace Day3.Model
{
    public class PetEntity : TableEntity
    {
        public string BlobUrl { get; set; }
    }
}