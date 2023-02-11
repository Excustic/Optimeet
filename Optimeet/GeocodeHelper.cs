using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
            int N = res["results"].Count();
            if (N > NumOfResults)
                while (N > NumOfResults && radius > 120)
                {
                    radius *= 0.8;
                    url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
                    Response = await GetAsync(url);
                    res = JsonConvert.DeserializeObject<JObject>(Response);
                    N = res["results"].Count();
                }
            else if (N == 0)
            {
                while (N == 0 && radius < 20000)
                {
                    radius *= 1.5;
                    url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
                    Response = await GetAsync(url);
                    res = JsonConvert.DeserializeObject<JObject>(Response);
                    N = res["results"].Count();
                }

            }
            Location[] suggestions = new Location[3];
            for (int i = 0; i < N; i++)
            {
                JObject j = JsonConvert.DeserializeObject<JObject>(res["results"][i].ToString());
                suggestions[i].Latitude = float.Parse(j["geometry"]["location"]["lat"].ToString());
                suggestions[i].Longitude = float.Parse(j["geometry"]["location"]["lng"].ToString());
                suggestions[i].Address = j["vicinity"].ToString();
                suggestions[i].PhotoReference = j["reference"].ToString();
                suggestions[i].Name = j["name"].ToString();
            }
            return suggestions;
        }
        public async Task<string> GetAsync(string uri)
        {
            var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync(uri).ConfigureAwait(false);
            return response;
        }

    }
}
