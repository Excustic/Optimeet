using Optimeet;
using System;
using System.Device.Location;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Threading.Tasks;

[DataContract]
[KnownType(typeof(TrieNode<Contact>))]
[KnownType(typeof(Contact))]
public struct Location
{
    [DataMember]
    public float Latitude;
    [DataMember]
    public float Longitude;
    [DataMember]
    public string Address;
    [DataMember]
    public string Name;
    [DataMember]
    public string PhotoReference;
    [DataMember]
    public double Rating;
    [DataMember]
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
    [DataContract]
    [KnownType(typeof(TrieNode<Contact>))]
    public class Contact
    {
        [DataMember]
        private string _Name;
        [DataMember]
        public string Name
        {
            get { return _Name; }
            private set
            {
                if (value.Length > 1)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (!char.IsLetter(value[i]))
                            if (i < 2 || !char.IsWhiteSpace(value[i]))
                                throw new Exception("Invalied name");
                    }
                    _Name = value;
                }
                else
                {
                    throw new Exception("Invalid name");
                }
            }
        }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        private Location SavedLocation;
        public Contact(string name, string email = null)
        {
            Name = name;
            if(email != null || email.Length == 0)
                try
                {
                    _ = new MailAddress(email);
                    Email = email;
                }
                catch (Exception)
                {
                    throw new Exception("Invalid mail");
                }
        }
        public async void SetLocation(float lat, float lon, string Address="Address Unavailable", string Name = null, string PhotoReference = null)
        {
            SavedLocation.Latitude = lat;
            SavedLocation.Longitude = lon;
            SavedLocation.Address = Address;
            if (Address==null||Address.Equals("none"))
                SavedLocation.Address = await MapsHelper.GetInstance().ReverseGeocode(lat, lon);
            SavedLocation.Name = Name;
            SavedLocation.PhotoReference = PhotoReference;
        }
        public void SetLocation(Location l)
        {
            SavedLocation = l;
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
