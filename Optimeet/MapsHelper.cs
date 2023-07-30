using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Optimeet
{
    /// <summary>
    /// A singleton class which works with Google Maps Api and processes geodata
    /// </summary>
    public sealed class MapsHelper
    {
        //All of the essential api urls
        private static string baseUrlGC = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key=";
        private static string baseUrlRGC = "http://api.positionstack.com/v1/reverse?access_key={0}&query={1},{2}&fields=results.label&limit=1";
        private static string baseUrlPR = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?keyword={0}&location={1},{2}&radius={3}&key=";
        private static string baseUrlPhotoRequest = "https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&maxheight=400&photo_reference={0}&key=";
        private static string AutocompleteRequest = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types={1}&key=";
        //Api Keys that are stored in 'keys.json' file
        private static string ApiKey;
        private static string GKey;
        //Strings that are used across several functions
        const string ReviewCount = "user_ratings_total";
        const string ratings = "rating";

        private MapsHelper() { }
        private static MapsHelper _instance;
        /// <summary>
        /// Initializes the singleton object
        /// </summary>
        /// <returns>A <see cref="MapsHelper"/> instance</returns>
        public static MapsHelper GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MapsHelper();
                Initialzie();
            }
            return _instance;
        }
        /// <summary>
        /// Gets the API keys from the stored file and appends them to the url strings
        /// </summary>
        private static void Initialzie()
        {
            try
            {
                using (var reader = new StreamReader(FileManager.path_keys))
                {
                    while (!reader.EndOfStream)
                    {
                        var data = reader.ReadToEnd();
                        JObject file = JsonConvert.DeserializeObject<JObject>(data);
                        ApiKey = file["ApiKey"].ToString();
                        GKey = file["GKey"].ToString();
                    }
                    baseUrlGC += GKey;
                    baseUrlPR += GKey;
                    baseUrlPhotoRequest += GKey;
                    AutocompleteRequest += GKey;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not load all map services because some keys are missing", "Error");
            }
        }
        /// <summary>
        /// Converts an address into a WGS84 coordinates
        /// </summary>
        /// <param name="address"></param>
        /// <returns>The converted coordinates in a <see cref="Location"/> object</returns>
        public async Task<Location> Geocode(string address)
        {
            try
            {
                var url = string.Format(baseUrlGC, address);
                string result = await GetAsync(url);
                JObject res = JsonConvert.DeserializeObject<JObject>(result);
                // return Location struct by extracting the latitude and longitude from the JSON object
                return new Location()
                {
                    Latitude = float.Parse(res["results"][0]["geometry"]["location"]["lat"].ToString()),
                    Longitude = float.Parse(res["results"][0]["geometry"]["location"]["lng"].ToString())
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("ReverseGeocode failed: " + e.Message);
            }
            return new Location();
        }
        /// <summary>
        /// Converts WGS84 coordinates into a verbal address
        /// </summary>
        /// <param name="Latitude">A real number between -90 and 90</param>
        /// <param name="Longitude">A real number between -180 and 180</param>
        /// <returns>The converted address</returns>
        public async Task<string> ReverseGeocode(float Latitude, float Longitude)
        {
            try
            {
                var url = String.Format(baseUrlRGC, ApiKey, Latitude, Longitude);
                string result = await GetAsync(url);
                JObject res = JsonConvert.DeserializeObject<JObject>(result);
                return res["data"][0]["label"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("ReverseGeocode failed: " + e.Message);
            }
            return null;
        }
        /// <summary>
        /// Gives autocomplete suggestions for a certain address via Google Maps Autocomplete API
        /// </summary>
        /// <param name="address"></param>
        /// <returns>The autocomplete results for the given address</returns>
        public async Task<List<string>> SearchLocation(string address)
        {
            try
            {
                var url = string.Format(AutocompleteRequest, address, "geocode");
                string result = await GetAsync(url);
                JArray res = JArray.Parse(JsonConvert.DeserializeObject<JObject>(result)["predictions"].ToString()); //Nested serialization inside another - this gets the list of outputs only
                List<string> PlaceResults = new List<string>();
                foreach (JObject obj in res)
                    PlaceResults.Add(obj["description"].ToString());
                return PlaceResults;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Please check your internet connection and try again", "Error - Could not connect to services");
            }
            catch (Exception e)
            {
                Console.Write("Error occured: " + e.Message);
            }
            return null;
        }
        /// <summary>
        /// Retrieves places around a location and sorts them by distance in an ascending order
        /// </summary>
        /// <param name="l">A location which nearby places are retrieved</param>
        /// <param name="NumOfResults">Esentially N, the number of the returned array</param>
        /// <param name="radius">Maximum distance from the location that the places nearby will be retrieved</param>
        /// <param name="filter">Category of places (Park, bar, etc.)</param>
        /// <returns>A location array of the size N which contains the sorted places</returns>
        public async Task<Location[]> TopNLocations(Location l, int NumOfResults, double radius, string filter = "")
        {
            try
            {
                string url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
                string Response = await GetAsync(url);
                JObject res = JsonConvert.DeserializeObject<JObject>(Response);
                Location[] Reccomendations = GetPlaces(res);
                while (Reccomendations.Length < NumOfResults && (Reccomendations.Length == 0 || radius < 20000))
                {
                    radius *= 1.5;
                    url = string.Format(baseUrlPR, filter, l.Latitude, l.Longitude, radius);
                    Response = await GetAsync(url);
                    res = JsonConvert.DeserializeObject<JObject>(Response);
                    Reccomendations = GetPlaces(res);
                }
                if (Reccomendations.Length > NumOfResults)
                    Reccomendations = SortPlacesByDistance(Reccomendations, l, NumOfResults);
                return Reccomendations;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Please check your internet connection and try again", "Error - Could not connect to services");
                return new Location[] { };
            }
        }
        /// <summary>
        /// Compares locations to a central point and sorts them correspondingly
        /// </summary>
        /// <param name="reccomendations">The set of locations which are compared</param>
        /// <param name="Centroid">The central point which the locations are compared with</param>
        /// <param name="numOfResults">The number of locations that will be in the final set, the rest are cut off</param>
        /// <returns>The array of the sorted locations by distance</returns>
        private Location[] SortPlacesByDistance(Location[] reccomendations, Location Centroid, int numOfResults)
        {
            Dictionary<double, Location> d = new Dictionary<double, Location>();
            double[] dist = new double[reccomendations.Length];
            for (int i = 0; i < reccomendations.Length; i++)
            {
                dist[i] = reccomendations[i].DistanceTo(Centroid);
                while (d.ContainsKey(dist[i]))
                    dist[i] += 0.1;
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
        /// <summary>
        /// Converts a JSON object to an array of <see cref="Location"/> objects
        /// </summary>
        /// <param name="LocationsJson"></param>
        /// <returns>The converted <see cref="Location"/> objects</returns>
        private Location[] GetPlaces(JObject LocationsJson)
        {
            Location[] temp = new Location[LocationsJson["results"].Count()];
            JArray Locations = (JArray)LocationsJson["results"];
            int open = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                JObject j = JsonConvert.DeserializeObject<JObject>(Locations[i].ToString());
                if (j[ratings] != null && j[ReviewCount] != null && double.Parse(j[ratings].ToString()) >= 3 && int.Parse(j[ReviewCount].ToString()) > 40)
                    temp[open++] = ConvertJsonToLocation(j);
            }
            Location[] Places = new Location[open];
            for (int i = 0; i < open; i++)
            {
                Places[i] = temp[i];
            }
            return Places;
        }
        /// <summary>
        /// Converts a JSON to a singular <see cref="Location"/> object
        /// </summary>
        /// <param name="j"></param>
        /// <returns>The converted <see cref="Location"/> object</returns>
        private Location ConvertJsonToLocation(JObject j)
        {
            Location loc = new Location();
            loc.Latitude = float.Parse(j["geometry"]["location"]["lat"].ToString());
            loc.Longitude = float.Parse(j["geometry"]["location"]["lng"].ToString());
            loc.Address = j["vicinity"].ToString();
            loc.PhotoReference = j["photos"][0]["photo_reference"].ToString();
            loc.Name = j["name"].ToString();
            loc.Rating = double.Parse(j[ratings].ToString());
            loc.ReviewCount = int.Parse(j[ReviewCount].ToString());
            return loc;
        }
        /// <summary>
        /// Sends a GET request asynchronously via an <see cref="HttpClient"/>
        /// </summary>
        /// <param name="uri">A URL string</param>
        /// <returns>A <see cref="string"/> response</returns>
        public async Task<string> GetAsync(string uri)
        {
            var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync(uri).ConfigureAwait(false);
            return response;
        }
        /// <summary>
        /// Retrieves an image of a place from a URL using Google API
        /// </summary>
        /// <param name="photoReference">A child string of a Google Place JSON object saved in a <see cref="Location"/> object that links to a cover photo of the place</param>
        /// <returns>Returns a <see cref="BitmapImage"/> instance</returns>
        public async Task<BitmapImage> BitmapImageFromUrl(string photoReference)
        {
            BitmapImage bitmap = null;
            var httpClient = new HttpClient();

            using (var response = await httpClient.GetAsync(string.Format(baseUrlPhotoRequest, photoReference)))
            {
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = new MemoryStream())
                    {
                        await response.Content.CopyToAsync(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
            }

            return bitmap;
        }
    }
}
