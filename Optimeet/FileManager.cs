using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Optimeet
{
    public sealed class FileManager
    {
        readonly string path_contacts = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "contacts.xml");
        readonly string path_meetings = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "meetings.xml");
        readonly string path_settings = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "settings.csv");
        public Trie<Contact> Contacts;
        public SortedSet<Meeting> Meetings;
        public Dictionary<string, int[]> Settings;
        private static FileManager _instance;
        public const string SETTING_1 = "Number of place suggestions";
        public const string SETTING_2 = "Search radius (metres)";
        public const string SETTING_3 = "Meeting duration (minutes)";
        public const string path_keys = "keys.json";
        private FileManager() 
        {
            LoadContacts();
            LoadMeetings();
            LoadSettings();
        }

        public static FileManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FileManager();
            }
            return _instance;
        }


        private void LoadSettings()
        {
            Settings = new Dictionary<string, int[]>();
            try
            {
                using(var reader = new StreamReader(path_settings))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        int[] arr = new int[] {
                        int.Parse(values[1]),
                        int.Parse(values[2]),
                        int.Parse(values[3]),
                        int.Parse(values[4])};
                        Settings.Add(values[0], arr);
                    }
                }
            }
            catch(FileNotFoundException)
            {
                //Name, [minimum, default, maximum, frequency(tick)]
                Settings.Add(SETTING_1, new int[] { 3, 5, 15, 1});
                Settings.Add(SETTING_2, new int[] { 300, 800, 5000, 100});
                Settings.Add(SETTING_3, new int[] { 30, 60, 300, 30});
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            if (!Directory.Exists(path_meetings))
            {
                string[] splits = path_meetings.Split('\\');
                string rootpath = path_meetings.Substring(0, path_meetings.Length - splits[splits.Length - 1].Length);
                Directory.CreateDirectory(rootpath);
            }
            string csv = string.Join(Environment.NewLine, Settings.Select(d => $"{d.Key},{d.Value[0]},{d.Value[1]},{d.Value[2]},{d.Value[3]}"));
            File.WriteAllText(path_settings, csv);
        }

    private void LoadContacts()
        {
            if (File.Exists(path_contacts))
            {
                var fileStream = new FileStream(path_contacts, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas() { MaxDepth = 200});
                var serializer = new DataContractSerializer(typeof(Trie<Contact>));
                Trie<Contact> serializableObject = (Trie<Contact>)serializer.ReadObject(reader, true);
                reader.Close();
                fileStream.Close();
                Contacts = serializableObject;
            }
            else Contacts = new Trie<Contact>();
        }
        public void SaveContacts()
        {
            var serializer = new DataContractSerializer(typeof(Trie<Contact>));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                
            };
            if (!Directory.Exists(path_contacts))
            { 
                string[] splits = path_contacts.Split('\\');
                string rootpath = path_contacts.Substring(0, path_contacts.Length - splits[splits.Length - 1].Length);
                Directory.CreateDirectory(rootpath);
            }
            var writer = XmlWriter.Create(path_contacts, settings);
            serializer.WriteObject(writer, Contacts);
            writer.Close();
        }
        private void LoadMeetings()
        {
            if (File.Exists(path_meetings))
            {
                var fileStream = new FileStream(path_meetings, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
                var serializer = new DataContractSerializer(typeof(SortedSet<Meeting>));
                SortedSet<Meeting> serializableObject = (SortedSet<Meeting>)serializer.ReadObject(reader, true);
                reader.Close();
                fileStream.Close();
                Meetings = serializableObject;
            }
            else Meetings = new SortedSet<Meeting>(new MeetingComparer());
        }


        public void SaveMeetings()
        {
            var serializer = new DataContractSerializer(typeof(SortedSet<Meeting>));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
            };
            if (!Directory.Exists(path_meetings))
            {
                string[] splits = path_meetings.Split('\\');
                string rootpath = path_meetings.Substring(0, path_meetings.Length - splits[splits.Length - 1].Length);
                Directory.CreateDirectory(rootpath);
            }
            var writer = XmlWriter.Create(path_meetings, settings);
            serializer.WriteObject(writer, Meetings);
            writer.Close();
        }      
    }

    internal class MeetingComparer : IComparer<Meeting>
    {
        public int Compare(Meeting m1, Meeting m2)
        {
            //Compares two meetings by date
            return DateTime.Compare(m1.GetMeetingDate(), m2.GetMeetingDate());
        }
    }
}
