using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Optimeet
{
    [DataContract]
    [KnownType(typeof(SortedSet<Meeting>))]
    public class Meeting:IComparable<Meeting>
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        private DateTime MeetingDate;
        [DataMember]
        private List<Contact> People;
        [DataMember]
        private Location MeetingLocation;
        [DataMember]
        public string googleId { get; set; }

        public Meeting(string t, DateTime d, List<Contact> p)
        {
            Title = t;
            MeetingDate = d;
            People = p;
        }
        public DateTime GetMeetingDate()
        {
            return MeetingDate;
        }
        public void SetMeetingDate(DateTime dt)
        {
            MeetingDate = dt;
        }
        public List<Contact> GetPeople()
        {
            return People;
        }
        public void AddPerson(Contact p)
        {
            if (p != null)
                People.Add(p);
            else throw new Exception("null exception");
        }
        public Location GetLocation()
        {
            return MeetingLocation;
        }
        public void SubmitLocation(Location FinalLoc)
        {
            MeetingLocation = FinalLoc;
        }

        public async Task<Location[]> SuggestLocations(string filter = "")
        {
            Location Centroid = LocationsCentroidWeighted();
            Location[] suggestions = await MapsHelper.GetInstance().TopNLocations(Centroid,
                FileManager.GetInstance().Settings[FileManager.SETTING_1][1],
                FileManager.GetInstance().Settings[FileManager.SETTING_2][1],
                filter);
            return suggestions;
        }

        private Location LocationsCentroidWeighted()
        {
            Location[] locations = new Location[People.Count];
            int N = locations.Length;
            float[] Avg = { 0.0f, 0.0f };
            float[] sigma = { 0.0f, 0.0f };
            Location Solution = new Location();
            for (int i = 0; i < locations.Length; i++)
            {
                locations[i] = People.ElementAt(i).GetLocation();
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
        public override string ToString()
        {
            string names = "";
            foreach (Contact item in People)
                names += item.Name + ",";
            names = names.Substring(0, names.Length - 2);
            return Title + " meeting, on the " + MeetingDate.ToString() + ", at " + MeetingLocation.Address + ". Attending: " + names;
        }
        public int CompareTo(Meeting m)
        {
            return DateTime.Compare(MeetingDate, m.MeetingDate);
        }
    }
}
