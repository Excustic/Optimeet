using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Optimeet
{
    /// <summary>
    /// Represents a meeting with properties for title, date and time, attendees, and location.
    /// </summary>
    [DataContract]
    [KnownType(typeof(SortedSet<Meeting>))]
    public class Meeting : IComparable<Meeting>
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        private DateTime MeetingDate;
        [DataMember]
        private List<Contact> Attendees;
        [DataMember]
        private Location MeetingLocation;
        [DataMember]
        public string googleId { get; set; }
        /// <summary>
        /// Initializes a new instance of the Meeting class with provided values.
        /// </summary>
        /// <param name="t">The title of the meeting.</param>
        /// <param name="d">The date and time of the meeting.</param>
        /// <param name="p">The list of attendees (contacts) for the meeting.</param>
        public Meeting(string t, DateTime d, List<Contact> p)
        {
            Title = t;
            MeetingDate = d;
            Attendees = p;
        }
        /// <summary>
        /// Returns the date and time of the meeting.
        /// </summary>
        public DateTime GetMeetingDate()
        {
            return MeetingDate;
        }
        /// <summary>
        /// Sets the date and time of the meeting.
        /// </summary>
        public void SetMeetingDate(DateTime dt)
        {
            MeetingDate = dt;
        }
        /// <summary>
        /// Returns the list of attendees (contacts) for the meeting.
        /// </summary>
        public List<Contact> GetAttendees()
        {
            return Attendees;
        }
        /// <summary>
        /// Adds a new attendee (contact) to the meeting.
        /// </summary>
        public void AddAttendee(Contact p)
        {
            if (p != null)
                Attendees.Add(p);
            else throw new Exception("null exception");
        }
        /// <summary>
        /// Returns the location of the meeting.
        /// </summary>
        public Location GetLocation()
        {
            return MeetingLocation;
        }
        /// <summary>
        /// Sets the final location for the meeting.
        /// </summary>
        public void SubmitLocation(Location FinalLoc)
        {
            MeetingLocation = FinalLoc;
        }
        /// <summary>
        /// Suggests locations for the meeting based on attendees' locations.
        /// </summary>
        public async Task<Location[]> SuggestLocations(string filter = "")
        {
            Location Centroid = LocationsCentroidWeighted();
            Location[] suggestions = await MapsHelper.GetInstance().TopNLocations(Centroid,
                FileManager.GetInstance().Settings[FileManager.SETTING_1][1],
                FileManager.GetInstance().Settings[FileManager.SETTING_2][1],
                filter);
            return suggestions;
        }
        /// <summary>
        /// Calculates the weighted centroid of attendees' locations.
        /// </summary>
        private Location LocationsCentroidWeighted()
        {
            Location[] locations = new Location[Attendees.Count];
            int N = locations.Length;
            float[] Avg = { 0.0f, 0.0f };
            float[] sigma = { 0.0f, 0.0f };
            Location Solution = new Location();
            for (int i = 0; i < locations.Length; i++)
            {
                locations[i] = Attendees.ElementAt(i).GetLocation();
            }
            foreach (Location l in locations)
            {
                Avg[0] += l.Latitude;
                Avg[1] += l.Longitude;
            }
            Avg[0] /= N;
            Avg[1] /= N;
            foreach (Location l in locations)
            {
                sigma[0] += (float)Math.Pow(l.Latitude - Avg[0], 2);
                sigma[1] += (float)Math.Pow(l.Longitude - Avg[1], 2);
            }
            sigma[0] = (float)Math.Sqrt(sigma[0] / N);
            sigma[1] = (float)Math.Sqrt(sigma[1] / N);

            foreach (Location l in locations)
            {
                Solution.Latitude += l.Latitude * ((float)Math.Pow(Math.E, (-Math.Pow(l.Latitude - Avg[0], 2)) / (2 * Math.Pow(sigma[0], 2))));
                Solution.Longitude += l.Longitude * ((float)Math.Pow(Math.E, (-Math.Pow(l.Longitude - Avg[1], 2)) / (2 * Math.Pow(sigma[1], 2))));
            }
            float SumLat = 0;
            float SumLon = 0;
            foreach (Location l in locations)
            {
                SumLat += (float)Math.Pow(Math.E, (-Math.Pow(l.Latitude - Avg[0], 2)) / (2 * Math.Pow(sigma[0], 2)));
                SumLon += (float)Math.Pow(Math.E, (-Math.Pow(l.Longitude - Avg[1], 2)) / (2 * Math.Pow(sigma[1], 2)));
            }
            Solution.Latitude /= SumLat;
            Solution.Longitude /= SumLon;

            return Solution;
        }
        /// <summary>
        /// Returns a string representation of the meeting.
        /// <para></para>
        /// <example>
        /// For example:
        /// <code>
        /// List&lt;Contact&gt; c = new List&lt;Contact&gt;();
        /// Contact c1 = new Contact("Andy Milonakis", "AndyMilonakis100@example.com");
        /// c1.SetLocation(40.832710, -74.108091, "175 Union Ave, NJ");
        /// Contact c2 = new Contact("Michael Kushnir", "Michael.Kushnir@example.com");
        /// c2.SetLocation(40.842284, -74.084991, "57 Columbia St, NJ");
        /// Meeting m = new Meeting("Andy's Birthday", new DateTime("10-08-2025"), c);
        /// Location meetingLocation = m.SuggestLocations()[0];
        /// m.SubmitLocation(meetingLocation);
        /// m.ToString();
        /// </code>
        /// Will output: Andy's Birthday meeting, on 10/08/2025 00:00, at 243 Laurel Pl, NJ. Attending: Andy Wilonakis, Michael Kushnir
        /// </example>
        /// </summary>
        public override string ToString()
        {
            string names = "";
            foreach (Contact item in Attendees)
                names += item.Name + ",";
            names = names.Substring(0, names.Length - 2);
            return Title + " meeting, on " + MeetingDate.ToString() + ", at " + MeetingLocation.Address + ". Attending: " + names;
        }
        /// <summary>
        /// Compares meetings based on their dates.
        /// </summary>
        public int CompareTo(Meeting m)
        {
            return DateTime.Compare(MeetingDate, m.MeetingDate);
        }
    }
    /// <summary>
    /// Used for comparing between <see cref="Meeting"/> objects when creating a SortedSet
    /// </summary>
    public class MeetingComparer : IComparer<Meeting>
    {
        /// <summary>
        /// Compares two meetings by date
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>Returns -1 if the first meeting is earlier, 0 if identical, 1 if it's later than the second meeting</returns>
        public int Compare(Meeting m1, Meeting m2)
        {
            return DateTime.Compare(m1.GetMeetingDate(), m2.GetMeetingDate());
        }
    }
}
