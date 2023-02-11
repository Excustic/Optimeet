using System;
using System.Collections.Generic;
using System.Linq;

namespace Optimeet
{
    class Meeting
    {
        public struct Date
        {
            public int Day;
            public int Month;
            public int Year;
            public int Hour;
            public int Minute;
            public override string ToString()
            {
                return Day + "/" + Month + "/" + Year + " " + Hour + ":" + Minute;

            }
        }
        private string Title;
        private Date MeetingDate;
        private List<Person> People;
        private Location MeetingLocation;
        public Meeting(string t, Date d, List<Person> p)
        {
            Title = t;
            MeetingDate = d;
            People = p;
        }
        public void AddPerson(Person p)
        {
            if (p != null)
                People.Add(p);
            else throw new Exception("null exception");
        }
        public void SubmitLocation(Location FinalLoc)
        {
            MeetingLocation = FinalLoc;
        }

        public async void SuggestLocations(string filter = "")
        {
            Location Centroid = LocationsCentroidWeighted();
            Location[] suggestions = await GeocodeHelper.GetInstance().TopNLocations(Centroid, 3, filter);

        }

        private Location LocationsCentroidWeighted()
        {
            Location[] locations = new Location[People.Capacity];
            int N = locations.Length;
            float[] Avg = { 0.0f, 0.0f };
            float[] sigma = { 0.0f, 0.0f };
            Location Solution = new Location();
            for (int i = 0; i < People.Capacity; i++)
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
                sigma[0] += l.Latitude - Avg[0];
                sigma[1] += l.Longitude - Avg[1];
            }
            sigma[0] = (float)Math.Sqrt(sigma[0] / N);
            sigma[1] = (float)Math.Sqrt(sigma[1] / N);

            foreach (Location l in locations)
            {
                Solution.Latitude += l.Latitude * (1 + (float)Math.Pow(Math.E, Math.Pow(-(l.Latitude - Avg[0]), 2) / (2 * Math.Pow(sigma[0], 2))));
                Solution.Longitude += l.Longitude * (1 + (float)Math.Pow(Math.E, Math.Pow(-(l.Longitude - Avg[1]), 2) / (2 * Math.Pow(sigma[1], 2))));
            }
            Solution.Latitude /= 2 * N;
            Solution.Longitude /= 2 * N;

            return Solution;
        }
        public override string ToString()
        {
            string names = "";
            foreach (Person item in People)
                names += item.Name + ",";
            names = names.Substring(0, names.Length - 2);
            return Title + " meeting, on the " + MeetingDate.ToString() + ", at " + MeetingLocation.Address + ". Attending: " + names;
        }
    }
}
