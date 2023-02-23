using System;
using System.Device.Location;
using System.Linq;

public struct Location
{
    public float Latitude;
    public float Longitude;
    public string Address;
    public string Name;
    public string PhotoReference;
    public override string ToString()
    {
        return Name + " at " + Address;
    }
    public double DistanceTo(Location l2)
    {
        var g1 = new GeoCoordinate(Latitude, Longitude);
        var g2 = new GeoCoordinate(l2.Latitude, l2.Longitude);
        return g1.GetDistanceTo(g2);
    }

}
namespace Optimeet
{
    class Person
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            private set
            {
                if (value.Length != 0 && value.All(Char.IsLetter))
                {
                    _Name = value;
                }
                else
                {
                    throw new Exception("Invalid name");
                }
            }
        }
        private Location SavedLocation;
        public Person(string n)
        {
            Name = n;
            SavedLocation = new Location();
        }
        public Person(string n, Location l)
        {
            Name = n;
            SavedLocation = l;
        }
        public void SetLocation(float lat, float lon, string Address, string Name = "null", string PhotoReference = "null")
        {
            SavedLocation.Latitude = lat;
            SavedLocation.Longitude = lon;
            SavedLocation.Address = Address;
            SavedLocation.Name = Name;
            SavedLocation.PhotoReference = PhotoReference;
        }
        public Location GetLocation()
        {
            return SavedLocation;
        }
        public void LatLongToAddress()
        {

        }
        public override string ToString()
        {
            string Address;
            Address = SavedLocation.Address == null ? "No available location" : SavedLocation.Address;
            return Name + ", " + SavedLocation.Address;
        }
    }
}
