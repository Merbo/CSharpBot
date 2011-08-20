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
        /// <summary>
        /// The command prefix
        /// </summary>
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

        /// <summary>
        /// The owner's hostmask
        /// </summary>
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

        /// <summary>
        /// The channel
        /// </summary>
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

        /// <summary>
        /// Use SSL connections?
        /// </summary>
        public bool SSL
        {
            get
            {
                try
                {
                    return bool.Parse(GetChildByName("ssl").InnerText);
                }
                catch
                {
                    Console.WriteLine("WARNING: Configuration file not updated. To use feature \"SSL\" you need to reset the configuration.");
                    return false;
                }
            }
            set
            {
                if (ConfigFile.SelectNodes("child::ssl").Count <= 0)
                    ConfigFile.AppendChild(ConfigFile.OwnerDocument.CreateElement("ssl"));
                GetChildByName("ssl").InnerText = value.ToString();
            }
        }

        /// <summary>
        /// The nickname
        /// </summary>
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

        /// <summary>
        /// The server
        /// </summary>
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

        /// <summary>
        /// The port
        /// </summary>
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

        /// <summary>
        /// The real name
        /// </summary>
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

        /// <summary>
        /// The server password
        /// </summary>
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

        /// <summary>
        /// The internal server's password
        /// </summary>
        public string LiveserverPassword
        {
            get
            {
                try
                {
                    return GetChildByName("liveserver").Attributes["password"].Value;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                if (LiveserverPassword == null) // No password attribute
                    GetChildByName("liveserver").Attributes.Append(ConfigFile.OwnerDocument.CreateAttribute("password"));
                if ((GetChildByName("liveserver").Attributes["password"].Value = value) == null) // To be removed
                    GetChildByName("liveserver").Attributes.Remove(GetChildByName("liveserver").Attributes["password"]);
            }
        }

        /// <summary>
        /// The NickServ account
        /// </summary>
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

        /// <summary>
        /// The NickServ password
        /// </summary>
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

        /// <summary>
        /// The dynamically combined complete USER line.
        /// </summary>
        [Obsolete("Use IrcFunctions.User to generate a userline.", false)]
        public string Userline
        {
            get { return "USER " + Nickname + " 8 * :" + Realname; }
        }

        /// <summary>
        /// Is file logging enabled?
        /// </summary>
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
                        GetChildByName("filelogging").Attributes.Append(ConfigFile.OwnerDocument.CreateAttribute("path"));
                        Logfile = "CSharpBot.log";
                    }
                }
                else
                {
                    if (ConfigFile.SelectNodes("child::filelogging").Count > 1)
                        ConfigFile.RemoveChild(ConfigFile.SelectNodes("child::filelogging")[0]);
                }
            }
        }

        /// <summary>
        /// The target file of logging
        /// </summary>
        public string Logfile
        {
            get
            {
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

        private XmlDocument Configuration;

        /// <summary>
        /// The configuration file node
        /// </summary>
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

        /// <summary>
        /// Constructs an XML configuration instance
        /// </summary>
        public XmlConfiguration()
        {
            Reset();
        }

        /// <summary>
        /// Loads an XML configuration
        /// </summary>
        /// <param name="file">Optional new file name</param>
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

        /// <summary>
        /// Saves the XML configuration
        /// </summary>
        /// <param name="file">Optional new file name</param>
        public void Save(string file = null)
        {
            Configuration.Save(OriginalFilePath = file != null ? file : OriginalFilePath);
        }

        /// <summary>
        /// Resets the configuration
        /// </summary>
        public void Reset()
        {
            Configuration = new XmlDocument();
            Configuration.AppendChild(Configuration.CreateElement("csharpbot")); // root node

            // child nodes of document
            CreateElement("prefix").InnerText = ".";
            CreateElement("ownerhostmask");
            CreateElement("port").InnerText = "6667";
            CreateElement("ssl").InnerText = false.ToString();
            CreateElement("nickname").InnerText = "CSharpBot";
            CreateElement("serverpassword");
            CreateElement("nickserv");
            CreateElement("server").InnerText = "irc.merbosmagic.co.cc";
            CreateElement("realname").InnerText = "CSharpBot";
            CreateElement("channel").InnerText = "#CSharpBot";
            CreateElement("liveserver");
        }

        /// <summary>
        /// Deletes the configuration file.
        /// </summary>
        public void Delete()
        {
            System.IO.File.Delete(OriginalFilePath);
        }
    }
}