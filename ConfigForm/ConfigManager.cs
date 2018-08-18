using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ConfigForm
{
    public static class ConfigManager
    {
        // Properties used for testing
        //public static string CPath = @"C:\VS2013\FTPSyncTestFolder";
        //public static string CHost = "ftp://192.168.2.3:21//";
        //public static string CLocal = @"C:\VS2013\FTPSyncTestFolder\";
        //public static string CUser = "honza";
        //public static string CPass = "pass";
        // FTP login properties
        //public static string Username { get; set; }
        //public static string Password { get; set; }


        public static string CConfigFile = "XML/ServiceConfig.xml";
        public static string HASH_FILE_SUFIX = "._MyMd5";
        public static double IndexInterval = 30000;


        // Last time indexing as made
        public static DateTime LastTimeIndexing { get; set; }

        // All FTP connections
        public static List<ConnectionPair> Connections { get; set; }

        // Selected connection
        public static ConnectionPair SelectedConnection { get; set; }


        /// <summary>Load XML Config file
        /// </summary>
        public static void Load()
        {
            try
            {
                XElement root = XElement.Load(System.Environment.CurrentDirectory + @"/XML/ServiceConfig.xml");

                IndexInterval = double.Parse(root.Element("IndexInterval").Value);

                Connections = new List<ConnectionPair>(
                (from elem in root.Element("Connections").Elements("Connection")
                     let User = elem.Attribute("User").Value
                     let Password = elem.Attribute("Password").Value
                     select new ConnectionPair(elem.Attribute("Host").Value, elem.Attribute("LocalDir").Value,
                         User, Password)
                         ));

                //if(Connections != null)
                //    SelectedConnection = Connections[0];
                
                LastTimeIndexing = DateTime.Parse(root.Element("LastTimeIndexing").Value);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        /// <summary>Save values from config form to XML File
        /// </summary>
        public static void Save()
        {
            try
            {

                XElement root = new XElement("Configuration",
                    new XElement("Connections",
                        from c in Connections
                            select new XElement("Connection", 
                            new XAttribute("User", c.User),
                            new XAttribute("Password", c.Password),
                            new XAttribute("Host", c.Host),
                            new XAttribute("LocalDir", c.LocalDir)
                            )
                        ),
                    new XElement("IndexInterval", IndexInterval.ToString()),
                    new XElement("LastTimeIndexing", LastTimeIndexing.ToString())

                    );
                root.Save(System.Environment.CurrentDirectory + @"/XML/ServiceConfig.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }


    }
}
