using Optimeet;
using System;
using System.Device.Location;
using System.Net.Mail;
using System.Runtime.Serialization;
/// <summary>
/// Represents a geographical location with latitude, longitude, address, name, photo reference, rating, and review count.
/// </summary>
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
    /// <summary>
    /// Returns a string representation of the location with name and address.
    /// </summary>
    public override string ToString()
    {
        return Name + " at " + Address;
    }
    /// <summary>
    /// Calculates the distance to another location using geographical coordinates.
    /// </summary>
    /// <param name="l2">The second location to calculate the distance to.</param>
    /// <returns>The distance in meters.</returns>
    public double DistanceTo(Location l2)
    {
        var g1 = new GeoCoordinate(Latitude, Longitude);
        var g2 = new GeoCoordinate(l2.Latitude, l2.Longitude);
        return g1.GetDistanceTo(g2);
    }

}
namespace Optimeet
{
    /// <summary>
    /// Represents a contact with a name, email, and location information.
    /// </summary>
    [DataContract]
    [KnownType(typeof(TrieNode<Contact>))]
    public class Contact
    {
        [DataMember]
        private string _Name;
        /// <summary>
        /// Gets or sets the name of the contact.
        /// The name must contain at least two characters and consist of letters (optional: white spaces after the first two characters).
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _Name; }
            private set
            {
                // Name must be at least two characters long
                if (value.Length > 1)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        // Name can only contain letters or white spaces after the first two characters
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
        /// <summary>
        /// Gets or sets the email address of the contact (optional).
        /// </summary>
        [DataMember]
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the saved location information of the contact.
        /// </summary>
        [DataMember]
        private Location SavedLocation;
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class with the specified name and optional email address.
        /// </summary>
        /// <param name="name">The name of the contact.</param>
        /// <param name="email">The email address of the contact (optional).</param>
        public Contact(string name, string email = null)
        {
            Name = name;
            if (email != null && email.Length != 0)
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
        /// <summary>
        /// Sets the location information of the contact using the specified latitude, longitude, and address.
        /// If the address is null or "none", it will be automatically reverse geocoded to get the address.
        /// Optional parameters: name and photo reference for the location.
        /// </summary>
        /// <param name="lat">The latitude of the location.</param>
        /// <param name="lon">The longitude of the location.</param>
        /// <param name="Address">The address of the location (optional).</param>
        /// <param name="Name">The name of the location (optional).</param>
        /// <param name="PhotoReference">The photo reference of the location (optional).</param>
        public async void SetLocation(float lat, float lon, string Address = "Address Unavailable", string Name = null, string PhotoReference = null)
        {
            SavedLocation.Latitude = lat;
            SavedLocation.Longitude = lon;
            SavedLocation.Address = Address;
            if (Address == null || Address.Equals("none"))
                SavedLocation.Address = await MapsHelper.GetInstance().ReverseGeocode(lat, lon);
            SavedLocation.Name = Name;
            SavedLocation.PhotoReference = PhotoReference;
        }
        /// <summary>
        /// Sets the location information of the contact using a provided Location object.
        /// </summary>
        /// <param name="l">The Location object representing the contact's location.</param>
        public void SetLocation(Location l)
        {
            SavedLocation = l;
        }
        /// <summary>
        /// Returns the location information of the contact.
        /// </summary>
        /// <returns>The Location object representing the contact's location.</returns>
        public Location GetLocation()
        {
            return SavedLocation;
        }
        /// <summary>
        /// Returns a string representation of the contact with the name and address.
        /// If the address is not available, it will show "address unavailable".
        /// <para></para>
        /// <example>
        /// For Example:
        /// <code>
        /// Contact c = new Contact("Alexandro");
        /// c.ToString();
        /// </code>
        /// Will output: Alexandro, address unavailable
        /// </example>
        /// </summary>
        /// <returns>A string representing the contact in the format "Name, Address".</returns>
        public override string ToString()
        {
            string Address;
            Address = SavedLocation.Address == null ? "address unavailable" : SavedLocation.Address;
            return Name + ", " + Address;
        }
    }
}
