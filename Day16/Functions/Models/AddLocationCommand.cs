namespace Day16.Models
{
    public class AddLocationCommand
    {
        public string Name { get; }
        public double Longitude { get; }
        public double Latitude { get; }

        public AddLocationCommand(string name, double longitude, double latitude)
        {
            Name = name;
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}