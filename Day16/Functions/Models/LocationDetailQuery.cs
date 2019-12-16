using System;

namespace Day16.Models
{
    public struct LocationDetailQuery : IEquatable<LocationDetailQuery>
    {
        public LocationDetailQuery(string id, string host, Location location)
        {
            Id = id;
            Host = host;
            Location = location;
        }

        public string Id { get;  }
        public string Host { get;  }
        public Location Location { get;  }

        public bool Equals(LocationDetailQuery other)
        {
            return Id == other.Id && Host == other.Host && Location.Equals(other.Location);
        }

        public override bool Equals(object obj)
        {
            return obj is LocationDetailQuery other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Host, Location);
        }

        public static bool operator ==(LocationDetailQuery query1, LocationDetailQuery query2)
        {
            return query1.Equals(query2);
        }

        public static bool operator !=(LocationDetailQuery query1, LocationDetailQuery query2)
        {
            return !(query1 == query2);
        }
    }

    public struct Location : IEquatable<Location>
    {
        public Location(string name, double latitude, double longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }

        public string Name { get;  }
        public double Latitude { get;  }
        public double Longitude { get;  }
        

        public bool Equals(Location other)
        {
            return Name == other.Name && Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            return obj is Location other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Latitude, Longitude);
        }
    }
}