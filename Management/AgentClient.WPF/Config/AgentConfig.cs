using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;

namespace SuperSocket.Management.AgentClient.Config
{
#if !SILVERLIGHT
   [Serializable]
#endif
    public class AgentConfig
    {
        [XmlArrayItem("Node")]
        public List<NodeConfig> Nodes { get; set; }

        private const string SAVE_PATH = "Agent.Config";


#if !SILVERLIGHT

        public static AgentConfig Load()
        {
            if (!File.Exists(SAVE_PATH))
            {
                var config = new AgentConfig();
                config.Nodes = new List<NodeConfig>();
                config.Save();
                return config;
            }

            return File.ReadAllText(SAVE_PATH, Encoding.UTF8).XmlDeserialize<AgentConfig>();
        }

        public void Save()
        {
            File.WriteAllText(SAVE_PATH, this.XmlSerialize(), Encoding.UTF8);
        }
    }

#else
        public static AgentConfig Load()
        {
            var configContent = LoadContent();

            if (string.IsNullOrEmpty(configContent))
            {
                var config = new AgentConfig();
                config.Nodes = new List<NodeConfig>();
                config.Save();
                return config;
            }

            return configContent.XmlDeserialize<AgentConfig>();
        }

        public static string LoadContent()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(SAVE_PATH))
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(SAVE_PATH, FileMode.Open, isf))
                    {
                        using (StreamReader sr = new StreamReader(isfs, Encoding.UTF8))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static void SaveContent(string content)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(SAVE_PATH, FileMode.Create, isf))
                {
                    using (StreamWriter sw = new StreamWriter(isfs, Encoding.UTF8))
                    {
                        sw.Write(content);
                        sw.Close();
                    }
                }
            }
        }

        public void Save()
        {
            SaveContent(this.XmlSerialize());
        }
    }
#endif
}
