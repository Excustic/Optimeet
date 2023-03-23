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
        readonly string path = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDoc‌​uments), "Optimeet", "contacts.xml");
        public Trie<Contact> Contacts;
        public List<Meeting> Meetings;
        private static FileManager _instance;
        private FileManager() 
        {
            LoadContacts();
            Meetings = null;
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
            if (!Directory.Exists(path))
            { 
                string[] splits = path.Split('\\');
                string rootpath = path.Substring(0, path.Length - splits[splits.Length - 1].Length);
                Directory.CreateDirectory(rootpath);
            }
            var writer = XmlWriter.Create(path, settings);
            serializer.WriteObject(writer, Contacts);
            writer.Close();
        }

        private void LoadContacts()
        {
            if (File.Exists(path))
            {
                var fileStream = new FileStream(path, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
                var serializer = new DataContractSerializer(typeof(Trie<Contact>));
                Trie<Contact> serializableObject = (Trie<Contact>)serializer.ReadObject(reader, true);
                reader.Close();
                fileStream.Close();
                Contacts = serializableObject;
            }
            else Contacts = new Trie<Contact>();
        }

        private void SaveMeetings()
        {

        }
    }
}
