using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Finder
{
    [Serializable]
    public class MySettings
    {
        private const string FileName = "settings.xml";
        private readonly static string FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
        public List<string> HistoryFolders { get; set; }
        public List<string> HistoryKeywords { get; set; }
        public List<string> HistoryExtensions { get; set; }

        private static MySettings _instance;
        public static MySettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = LoadFromFile();
                }
                return _instance;
            }
        }

        private static MySettings LoadFromFile()
        {
            if (!File.Exists(FilePath)) 
                return new MySettings
                {
                    HistoryFolders = new List<string>(),
                    HistoryKeywords = new List<string>(),
                    HistoryExtensions = new List<string>(),
                };
            try
            {
                using (var s = new FileStream(FilePath, FileMode.Open))
                {
                    var xs = new XmlSerializer(typeof(MySettings));
                    return xs.Deserialize(s) as MySettings;
                }
            }
            catch (Exception)
            {
                return new MySettings
                {
                    HistoryFolders = new List<string>(),
                    HistoryKeywords = new List<string>(),
                    HistoryExtensions = new List<string>(),
                };
            }
            
        }

        public void Save()
        {
            using (var s = new StreamWriter(FilePath))
            {
                var xs = new XmlSerializer(typeof(MySettings));
                xs.Serialize(s, this);
            }
        }

        public void Add(string folder, string keyword, string ext)
        {
            if (!HistoryFolders.Contains(folder))
                HistoryFolders.Add(folder);
            if (!HistoryKeywords.Contains(keyword))
                HistoryKeywords.Add(keyword);
            if (!HistoryExtensions.Contains(ext))
                HistoryExtensions.Add(ext);
            foreach (var history in new []{HistoryFolders, HistoryKeywords, HistoryExtensions})
            {
                while (history.Count >10)
                {
                    history.RemoveAt(0);
                }
            }
            Save();
        }
    }
}
