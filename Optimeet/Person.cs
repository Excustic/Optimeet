using System;
public struct Location
{
    public float Latitude;
    public float Longitude;
    public string Address;
    public string Name;
    public string PhotoReference;
}
namespace Optimeet
{
    class Person
    {

        public string Name
        {
            get { return this.Name; }
            private set
            {
                if (value.Length != 0 && System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9\x20]+$"))
                {
                    this.Name = value;
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
