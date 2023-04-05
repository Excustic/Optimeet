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
        public Trie<Contact> Contacts;
        public SortedSet<Meeting> Meetings;
        private static FileManager _instance;
        private FileManager() 
        {
            LoadContacts();
            LoadMeetings();
        }

        public static FileManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FileManager();
            }
            return _instance;
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

        private void LoadContacts()
        {
            if (File.Exists(path_contacts))
            {
                var fileStream = new FileStream(path_contacts, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
                var serializer = new DataContractSerializer(typeof(Trie<Contact>));
                Trie<Contact> serializableObject = (Trie<Contact>)serializer.ReadObject(reader, true);
                reader.Close();
                fileStream.Close();
                Contacts = serializableObject;
            }
            else Contacts = new Trie<Contact>();
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
            else Meetings = new SortedSet<Meeting>();
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
}
