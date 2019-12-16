namespace Day16.Models
{
    public class CreatePasadasCommand
    {
        public CreatePasadasCommand(string hostId, string locationId)
        {
            HostId = hostId;
            LocationId = locationId;
        }

        public string HostId { get; }
        public string LocationId { get; set; }
    }
}