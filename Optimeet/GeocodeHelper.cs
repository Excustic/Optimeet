using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Optimeet
{
    public sealed class GeocodeHelper
    {
        private static string baseUrlGC = "http://api.positionstack.com/v1/reverse?access_key={0}&query={1},{2}&fields=results.label&limit=1"; // part1 of the URL for GeoCoding
        private static string baseUrlPR = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?keyword={0}&location={1},{2}&radius={3}&key=" + GKey;
        const string ApiKey = "2b06ea680225d5c6118150af3a7add6a";
        const string GKey = "AIzaSyCBv1-o_dBlNgVaX22qMTN-qwRRzCufkrM";
        const string ReviewCount = "user_ratings_total";
        const string ratings = "rating";

        private GeocodeHelper() { }
        private static GeocodeHelper _instance;
        public static GeocodeHelper GetInstance()
        {
            if (_instance == null)
                _instance = new GeocodeHelper();
            return _instance;
        }
        public async Task<string> ReverseGeocode(float Latitude, float Longitude)
        {
            string Address;
            Address = "";
            var url = String.Format(baseUrlGC, ApiKey, Latitude, Longitude);
            Console.WriteLine("waiting for response");
            string result = await GetAsync(url);
            Console.WriteLine(result);
            JObject res = JsonConvert.DeserializeObject<JObject>(result);
            Console.WriteLine(res["data"][0]["label"]);
            Console.WriteLine("printed response");
            return Address;
        }
        public async Task<Location[]> TopNLocations(Location l, int NumOfResults, string filter = "")
        {
            double radius = 1000;
            string url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
            string Response = await GetAsync(url);
            JObject res = JsonConvert.DeserializeObject<JObject>(Response);
            Location[] Reccomendations = GetPlaces(res);
            if (Reccomendations.Length < NumOfResults)
            {
                while (Reccomendations.Length == 0 && radius < 20000)
                {
                    radius *= 1.5;
                    url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
                    Response = await GetAsync(url);
                    res = JsonConvert.DeserializeObject<JObject>(Response);
                    Reccomendations = GetPlaces(res);
                }
            }
            if (Reccomendations.Length > NumOfResults)
                Reccomendations = SortPlacesByDistance(Reccomendations, l, NumOfResults);
            return Reccomendations;
        }

        private Location[] SortPlacesByDistance(Location[] reccomendations, Location Centroid, int numOfResults)
        {
            Dictionary<double, Location> d = new Dictionary<double, Location>();
            double[] dist = new double[reccomendations.Length];
            for (int i = 0; i < reccomendations.Length; i++)
            {
                dist[i] = reccomendations[i].DistanceTo(Centroid);
                d.Add(dist[i], reccomendations[i]);
            }
            Array.Sort(dist);
            Location[] TopN = new Location[numOfResults];
            for (int i = 0; i < TopN.Length; i++)
            {
                TopN[i] = d[dist[i]];
            }
            return TopN;
        }

        private Location[] GetPlaces(JObject LocationsJson)
        {
            Location[] temp = new Location[LocationsJson["results"].Count()];
            JArray Locations = (JArray)LocationsJson["results"];
            int open = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                JObject j = JsonConvert.DeserializeObject<JObject>(Locations[i].ToString());
                if (j[ratings]!=null && j[ReviewCount]!=null && double.Parse(j[ratings].ToString()) >= 3 && int.Parse(j[ReviewCount].ToString()) > 40)
                    temp[open++] = ConvertJsonToLocation(j);
            }
            Location[] Places = new Location[open];
            for (int i = 0; i < open; i++)
            {
                Places[i] = temp[i];
            }
            return Places;
        }

        private Location ConvertJsonToLocation(JObject j)
        {
            Location loc = new Location();
            loc.Latitude = float.Parse(j["geometry"]["location"]["lat"].ToString());
            loc.Longitude = float.Parse(j["geometry"]["location"]["lng"].ToString());
            loc.Address = j["vicinity"].ToString();
            loc.PhotoReference = j["reference"].ToString();
            loc.Name = j["name"].ToString();
            return loc;
        }

        public async Task<string> GetAsync(string uri)
        {
            var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync(uri).ConfigureAwait(false);
            return response;
        }

    }
}
