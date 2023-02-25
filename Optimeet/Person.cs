using System;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;

public struct Location
{
    public float Latitude;
    public float Longitude;
    public string Address;
    public string Name;
    public string PhotoReference;
    public double Rating;
    public int ReviewCount;
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
                if (value.Length > 1 && value.All(Char.IsLetter))
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
        public async void SetLocation(float lat, float lon, string Address="none", string Name = null, string PhotoReference = null)
        {
            SavedLocation.Latitude = lat;
            SavedLocation.Longitude = lon;
            SavedLocation.Address = Address;
            if (Address==null||Address.Equals("none"))
                SavedLocation.Address = await MapsHelper.GetInstance().ReverseGeocode(lat, lon);
            SavedLocation.Name = Name;
            SavedLocation.PhotoReference = PhotoReference;
        }
        public Location GetLocation()
        {
            return SavedLocation;
        }
        public override string ToString()
        {
            string Address;
            Address = SavedLocation.Address == null ? "Address unavailable" : SavedLocation.Address;
            return Name + ", " + Address;
        }
    }
}
