using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CSharpBot
{
    /// <summary>
    /// XML functions for CSharpBot configuration.
    /// </summary>
    public class XmlConfiguration
    {
        public string Prefix
        {
            get
            {
                return GetChildByName("prefix").InnerText;
            }
            set
            {
                GetChildByName("prefix").InnerText = value;
            }
        }
        public string OwnerHostMask
        {
            get
            {
                return GetChildByName("ownerhostmask").InnerText;
            }
            set
            {
                GetChildByName("ownerhostmask").InnerText = value;
            }
        }
        public string Channel
        {
            get
            {
                return GetChildByName("channel").InnerText;
            }
            set
            {
                GetChildByName("channel").InnerText = value;
            }
        }
        public string Nickname
        {

            get
            {
                return GetChildByName("nickname").InnerText;
            }
            set
            {
                GetChildByName("nickname").InnerText = value;
            }
        }
        public string Server
        {

            get
            {
                return GetChildByName("server").InnerText;
            }
            set
            {
                GetChildByName("server").InnerText = value;
            }
        }
        public int Port
        {
            get
            {
                return int.Parse(GetChildByName("port").InnerText);
            }
            set
            {
                GetChildByName("port").InnerText = value.ToString();
            }
        }
        public string Realname
        {
            get
            {
                return GetChildByName("realname").InnerText;
            }
            set
            {
                GetChildByName("realname").InnerText = value;
            }
        }
        public string ServerPassword
        {
            get
            {
                return GetChildByName("serverpassword").InnerText;
            }
            set
            {
                GetChildByName("serverpassword").InnerText = value;
            }
        }
        public string NickServAccount
        {
            get
            {
                return GetChildByName("nickserv").Attributes["accountname"].Value;
            }
            set
            {
                if (GetChildByName("nickserv").Attributes["accountname"] == null)
                    GetChildByName("nickserv").Attributes.Append(Configuration.CreateAttribute("accountname"));
                GetChildByName("nickserv").Attributes["accountname"].Value = value;
            }
        }
        public string NickServPassword
        {
            get
            {
                return GetChildByName("nickserv").Attributes["password"].Value;
            }
            set
            {
                if (GetChildByName("nickserv").Attributes["password"] == null)
                    GetChildByName("nickserv").Attributes.Append(Configuration.CreateAttribute("password"));
                GetChildByName("nickserv").Attributes["password"].Value = value;
            }
        }
        public string Userline
        {
            get { return "USER " + Nickname + " 8 * :" + Realname; }
        }
        public bool EnableFileLogging
        {
            get { return ConfigFile.SelectNodes("child::filelogging").Count > 1; }
            set
            {
                if (value)
                {
                    if (ConfigFile.SelectNodes("child::filelogging").Count == 0)
                    {
                        ConfigFile.AppendChild(ConfigFile.OwnerDocument.CreateElement("filelogging"));
                        GetChildByName("filelogging").Attributes.Append(ConfigFile.OwnerDocument.CreateAttribute("path")).Value = "CSharpBot.log";
                    }
                }
                else
                {
                    if (ConfigFile.SelectNodes("child::filelogging").Count > 1)
                        ConfigFile.RemoveChild(ConfigFile.SelectNodes("child::filelogging")[0]);
                }
            }
        }
        public string Logfile
        {
            get {
                if (EnableFileLogging)
                    return GetChildByName("filelogging").Attributes["path"].Value;
                else
                    return "";
            }
            set
            {
                if (EnableFileLogging)
                    GetChildByName("filelogging").Attributes["path"].Value = value;
            }
        }


        private XmlDocument Configuration = new XmlDocument();
        public XmlNode ConfigFile
        {
            get { return Configuration.SelectNodes("csharpbot")[0]; }
        }
        
        private string OriginalFilePath = "CSharpBot.xml";

        private XmlNode GetChildByName(string name)
        {
            return ConfigFile.SelectNodes("child::" + name)[0];
        }

        private XmlNode CreateElement(string name) { return ConfigFile.AppendChild(ConfigFile.OwnerDocument.CreateElement(name)); }

        public XmlConfiguration()
        {
            Configuration.AppendChild(Configuration.CreateElement("csharpbot")); // root node

            // child nodes of document
            CreateElement("prefix");
            CreateElement("ownerhostmask");
            CreateElement("port");
            CreateElement("nickname");
            CreateElement("serverpassword");
            CreateElement("nickserv");
            CreateElement("server");
            CreateElement("realname");
            CreateElement("channel");
        }

        public void Load(string file = null)
        {
            try
            {
                Configuration.Load(OriginalFilePath = file != null ? file : OriginalFilePath);
            }
            catch (Exception n)
            {
                Console.WriteLine("[xml] " + n.ToString());
                throw n;
            }
        }

        public void Save(string file = null)
        {
            Configuration.Save(OriginalFilePath = file != null ? file : OriginalFilePath);
        }
    }
}
