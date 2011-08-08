using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;
using System.Xml;

namespace CSharpBot
{
    public class CSharpBot
    {
        /// <summary>
        /// Our entry point for execution.
        /// </summary>
        /// <param name="args">Command line parameters</param>
        static void Main(string[] args)
        {
            Console.ResetColor();
            Console.WriteLine("CSharpBot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("\t(c) Merbo August 3, 2011-Present");
            Console.WriteLine("\t(c) Icedream August 5, 2011-Present");
            Console.WriteLine("\t(c) peapodamus August 7 2011-Present");
            Console.WriteLine();

            CSharpBot bot = new CSharpBot();
            bot.ParseArguments(args);
            bot.Run();
        }

        // Our RegEx'es
        public Regex HostmaskRegex;

        public StreamWriter writer;

        public bool DebuggingEnabled = false;
        public bool ProgramRestart = false;
        public bool RejoinOnKick = true;

        // Message of the day
        List<string> motdlines;
        public string MOTDText
        {
            get { return string.Join("\n", MOTDLines); }
        }
        public string[] MOTDLines
        {
            get { return motdlines.ToArray(); }
        }

        NetworkStream stream;
        TcpClient irc;
        StreamReader reader;

        #region XML implementation
        public XmlConfiguration config;
        public string XmlFileName = "CSharpBot.xml";

        // Originally, configuration was saved in plain text.
        // For our comfort, I'll just dynamically replace the old things with the XML settings.
        string NICK
        {
            get { return config.Nickname; }
            set { config.Nickname = value; }
        }
        string CHANNEL
        {
            get { return config.Channel; }
            set { config.Channel = value; }
        }
        string prefix
        {
            get { return config.Prefix; }
            set { config.Prefix = value; }
        }
        string SERVER
        {
            get { return config.Server; }
            set { config.Server = value; }
        }
        bool logging
        {
            get { return config.EnableFileLogging; }
            set { config.EnableFileLogging = value; }
        }
        #endregion   

        public DateTime startupTime = DateTime.Now;
        private IrcFunctions _functionsCached;
        public IrcFunctions Functions
        {
            get { if (_functionsCached != null) return _functionsCached; else return _functionsCached = new IrcFunctions(this); }
        }
        static void Usage()
        {
            Console.WriteLine("Usage: CSharpBot.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("--help, -h");
            Console.WriteLine("\tThis page.");
            Console.WriteLine("--config-file=config.xml, -f=config.xml");
            Console.WriteLine("\tUses config.xml as the configuration file. If this file does not exist, it will be created by the first-use setup.");
        }

        public void OnCtcpRequest(IrcMessageLine msg)
        {
        }

        public void OnChannelMessage(IrcMessageLine msg)
        {
        }

        public void OnPrivateMessage(IrcMessageLine msg)
        {
        }

        public delegate void KickedHandler(CSharpBot bot, string source, string channel);
        public event KickedHandler Kicked;
        public void OnKicked(string source, string channel)
        {
            if (Kicked != null)
                Kicked(this, source, channel);
        }

        public delegate void UserKickedHandler(CSharpBot bot, string source, string channel, string target);
        public event UserKickedHandler UserKicked;
        public void OnUserKicked(string source, string channel, string target)
        {
            if (UserKicked != null)
                UserKicked(this, source, channel, target);
        }

        public void OnNotice(IrcMessageLine msg)
        {
        }

        public delegate void NumericReplyReceivedHandler(CSharpBot bot, IrcNumericReplyLine reply);
        public event NumericReplyReceivedHandler NumericReplyReceived;
        public void OnNumericReplyReceived(IrcNumericReplyLine reply)
        {
            if (this.NumericReplyReceived != null)
                this.NumericReplyReceived(this, reply);
        }

        public void DoFirstUseSetup()
        {

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("=== First Use Configuration ===");
            Console.WriteLine("");
            Console.ResetColor();

            // The SERVER
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Server: ");
            Console.ForegroundColor = ConsoleColor.White;
            config.Server = Console.ReadLine();

            // The PORT
            Console.ForegroundColor = ConsoleColor.Cyan;
            int port = -1;
            while (port < 0 || port > 0xffff)
            {
                Console.Write("Port: ");

                // Errors?
                Console.ForegroundColor = ConsoleColor.White;
                bool validNumber = int.TryParse(Console.ReadLine(), out port);
                Console.ForegroundColor = ConsoleColor.Red;
                if (!validNumber)
                    Console.WriteLine("Sorry, but this is an invalid port number!");
                if (port > 0xffff)
                    Console.WriteLine("Sorry, but this is a too big number.");
                if (port < 0)
                    Console.WriteLine("Sorry, but this is a too small number.");
            }
            config.Port = port;

            // The NICKname
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Nick: ");
            Console.ForegroundColor = ConsoleColor.White;
            config.Nickname = Console.ReadLine();

            // The server password
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Server password (optional): ");
            Console.ForegroundColor = ConsoleColor.White;
            config.ServerPassword = Console.ReadLine();

            // The nickserv account
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("NickServ account (If different than nick): ");
            Console.ForegroundColor = ConsoleColor.White;
            config.NickServAccount = Console.ReadLine();

            // The nickserv password
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("NickServ password (optional): ");
            Console.ForegroundColor = ConsoleColor.White;
            config.NickServPassword = Console.ReadLine();

            // The CHANNEL
            while (!config.Channel.StartsWith("#"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Channel: ");
                Console.ForegroundColor = ConsoleColor.White;
                config.Channel = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (!config.Channel.StartsWith("#"))
                    Console.WriteLine("Sorry, but channel names always begin with #!");
            }

            // The ownerhost
            while (HostmaskRegex == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Hostmask of owner (Nickname!Username@Host, can be a regex globmask (wildcards like * and ?)): ");
                Console.ForegroundColor = ConsoleColor.White;
                config.OwnerHostMask = Console.ReadLine();
                try
                {
                    HostmaskRegex = new Regex(config.OwnerHostMask = "^" + config.OwnerHostMask.Replace(".", "\\.").Replace("*", ".+") + "$");
                    if (DebuggingEnabled == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("(debug) Parsed Regex: " + HostmaskRegex);
                        Console.ResetColor();
                    }
                }
                catch (Exception n)
                {
                    Functions.Log("Something went wrong: " + n.Message);
                    Functions.Log("Exception: " + n.ToString());
                    Functions.Log("StackTrace: " + n.StackTrace);
                }
            }

            // The prefix
            while (config.Prefix.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Command Prefix (e.g. in '!kick' it is '!'): ");
                Console.ForegroundColor = ConsoleColor.White;
                config.Prefix = Console.ReadKey().KeyChar.ToString();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                if (config.Prefix.Length < 1)
                    Console.WriteLine("You must set a prefix!");

            }

                //enable logging?
        retrylogging:
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enable logging? ([Y]es/[N]o) ");
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleKeyInfo yn = Console.ReadKey();
            Console.WriteLine();
            if (yn.Key == ConsoleKey.Y)
            {
                config.EnableFileLogging = true;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Where to log [Default: " + config.Logfile + "]? ");
                Console.ForegroundColor = ConsoleColor.White;
                string path = Console.ReadLine();
                if (path.Trim() != "")
                    config.Logfile = path;
            }
            else if (yn.Key == ConsoleKey.N)
            {
                config.EnableFileLogging = false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You must specify Yes or No!");
                goto retrylogging;
            }
            // Finishing configuration...
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                config.Save(XmlFileName);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration has been saved successfully to " + XmlFileName + ". The bot will now start!");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Configuration has NOT been saved. Please check if the directory is writeable for the bot.");
                Console.WriteLine("Enter something to exit.");
                Console.ReadKey();
                return;
            }
        }

        public void Connect()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Functions.Log("Connecting to " + config.Server + "...");

            irc = new TcpClient(config.Server, config.Port);
            if (!irc.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Functions.Log("Connection failed. Bot is restarting in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                ProgramRestart = true;
            }

            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        public void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                string[] parameters = arg.Split('=');
                string name = parameters[0].ToLower();
                string value = string.Join("=", parameters.Skip(1).ToArray());
                switch (name)
                {
                    case "-h":
                    case "--help":
                        Usage();
                        return;
                    case "-d":
                    case "--debug":
                        DebuggingEnabled = true;
                        break;
                    case "-f":
                    case "--config-file":
                        XmlFileName = value;                       
                        break;
                }
            }
        }

        public void Run()
        {
            this.NumericReplyReceived += new NumericReplyReceivedHandler(CSharpBot_NumericReplyReceived);
            this.Kicked += new KickedHandler(CSharpBot_Kicked);
        start: // This is the point at which the bot restarts on errors

            if (ProgramRestart == true)
            {
                Console.WriteLine("");
                HostmaskRegex = null;
                ProgramRestart = false;
            }

            config = new XmlConfiguration();

            string inputline;

            // Configuration
            if (!File.Exists(XmlFileName))
            {
                DoFirstUseSetup();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Loading configuration...");
                try
                {
                    config.Load(XmlFileName);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Configuration has NOT been loaded. Please check if the configuration is valid and try again.");
                    if (DebuggingEnabled == true)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(e.ToString());
                        Console.WriteLine(e.StackTrace);
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine(e.Message);
                    }
                    Console.WriteLine("Enter something to exit.");
                    Console.ReadKey();
                    return;
                }
            }

            // IRC
            try
            {
                Console.WriteLine();
                Connect();

                Functions.Log("Logging in...");
                Functions.User(config.Nickname, config.Realname);
                Functions.Nick(config.Nickname);
                string whoistarget;
                int replycode;
                while ((inputline = reader.ReadLine()) != null)
                {
                    string[] cmd = inputline.Split(' ');
                    if (DebuggingEnabled)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (!cmd[0].Equals("PING"))
                            Functions.Log("RECV: " + inputline);
                        Console.ResetColor();
                    }
                    if (cmd[0].Equals("PING"))
                    {
                        if (DebuggingEnabled == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Functions.Log("Ping? Pong!");
                            Console.ResetColor();
                        }
                        Functions.WriteData("PONG " + cmd[1]);
                    }

                    // Numeric replies
                    else if (int.TryParse(cmd[1], out replycode))
                    {
                        IrcNumericReplyLine reply = new IrcNumericReplyLine(inputline);
                        this.OnNumericReplyReceived(reply);
                    }

                    else if (cmd[1].Equals("KICK"))
                    {
                        if (cmd[3] == NICK)
                            OnKicked(cmd[0], cmd[2]);
                        else
                            OnUserKicked(cmd[0], cmd[2], cmd[3]);
                    }

                    // Handling incoming messages
                    IrcMessageLine msg = null;
                    try
                    {
                        msg = new IrcMessageLine(inputline, config);
                    }
                    catch (Exception)
                    {
                        // Do nothing. DON'T DELETE THIS LINE! It prevents errors during compilation.
                    }
                    if (msg != null)
                    {
                        #region PRIVMSG
                        if (msg.MessageType == IrcMessageType.PrivateMessage)
                        {
                            if (msg.Target.StartsWith("#"))
                            // Source is really a channel
                            {

                                // Execute commands
                                if (cmd[3] == ":" + prefix + "test")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "test");
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": I think your test works ;-)");
                                }
                                else if (cmd[3] == ":" + prefix + "amiowner")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "amiowner");
                                    Functions.PrivateMessage(msg.Target, "The answer is: " + (Functions.IsOwner(msg.SourceHostmask) ? "Yes!" : "No!"));
                                }
                                else if (cmd[3] == ":" + prefix + "addbotop")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {

                                        Botop add = new Botop();
                                        if (cmd.Length > 4)
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "addbotop");
                                            add.AddBotOp(cmd[4]);
                                            Functions.PrivateMessage(msg.Target, "Done!");
                                        }
                                        
                                    }
                                    else
                                    {
                                        Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": You aren't my owner!");
                                    }
                                }

                                else if (cmd[3] == ":" + prefix + "delbotop")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {

                                        Botop del = new Botop();
                                        if (cmd.Length > 4)
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "delbotop");
                                            del.DelBotOp(cmd[4]);
                                            Functions.PrivateMessage(msg.Target, "Done!");
                                        }

                                    }
                                    else
                                    {
                                        Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": You aren't my owner!");
                                    }
                                }


                                else if (cmd[3] == ":" + prefix + "amibotop")
                                {
                                    
                                    Botop test = new Botop();
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "amibotop");
                                    Functions.PrivateMessage(msg.Target, "The answer is: " + (test.isBotOp(msg.SourceNickname) ? "Yes!" : "No!"));
                                }
                                else if (cmd[3] == ":" + prefix + "uptime")
                                {
                                    TimeSpan ts = DateTime.Now - startupTime;
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": The bot is now running " + ts.ToString());
                                }
                                else if (cmd[3] == ":" + prefix + "time")
                                {
                                    // UTC hours addition (#time +1 makes UTC+1 for example)
                                    double add = 0;
                                    string adds = "";
                                    if (cmd.Length > 4)
                                        double.TryParse(cmd[4], out add);
                                    if (add != 0)
                                        adds = add.ToString();
                                    if (add > 0)
                                        adds = "+" + adds;

                                    if (cmd.Length > 4)
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "time " + cmd[4]);
                                    else
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "time");
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": It's " + DateTime.UtcNow.AddHours(add).ToString() + "(UTC" + adds + ")");
                                }
                                else if (cmd[3] == ":" + prefix + "mynick")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "mynick");
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": Your nick is " + msg.SourceNickname);
                                }
                                else if (cmd[3] == ":" + prefix + "myident")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "myident");
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": Your ident is " + msg.SourceUsername);
                                }
                                else if (cmd[3] == ":" + prefix + "myhost")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "myhost");
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": Your host is " + msg.SourceHost);
                                }
                                else if (cmd[3] == ":" + prefix + "myfullmask")
                                {
                                    Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": Your full mask is " + cmd[0]);
                                }
                                else if (cmd[3] == ":" + prefix + "die")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        if (cmd.Length > 4)
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "die " + string.Join(" ", cmd.Skip(5).ToArray()));
                                            Functions.Quit(string.Join(" ", cmd.Skip(5).ToArray()));
                                        }
                                        else
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "die");
                                            Functions.Quit("I shot myself because " + msg.SourceNickname + " told me to.");
                                        }
                                    }
                                    else
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "die");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "clean")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        FileInfo fi = new FileInfo("CSharpBot.xml");
                                        fi.Delete();
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "clean");
                                        Functions.Quit("Cleaned!");
                                    }
                                    else
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "clean");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "raw")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        if (cmd.Length > 4)
                                            Functions.Raw(string.Join(" ", cmd.Skip(4).ToArray()));
                                    }
                                    else
                                    {
                                        if (cmd.Length > 4)
                                            Functions.Log(msg.SourceNickname + " attempted to use " + prefix + " RAW " + string.Join(" ", cmd.Skip(4).ToArray()));
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "config")
                                {
                                    if (!Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "config");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                    } else if (cmd[4] == "list")
                                    {
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "config list");
                                        Regex search = new Regex("^(.*)$");
                                        if(cmd.Length > 5)
                                            search = new Regex(!cmd[5].StartsWith("regex:") ? "(" + cmd[5].Replace(".", "\\.") + ")" : cmd[5].Substring(6));
                                        foreach (XmlNode node in config.ConfigFile.ChildNodes)
                                        {
                                            // Main settings
                                            if (node.InnerText.Trim() != ""
                                                && (config.NickServPassword != "" ? !node.InnerText.Contains(config.NickServPassword) : true)
                                                && (config.ServerPassword != "" ? !node.InnerText.Contains(config.ServerPassword) : true)
                                                && search.Match(node.Name).Success
                                                )
                                                Functions.Notice(msg.SourceNickname, node.Name + " = " + node.InnerText);
                                            if (node.HasChildNodes)
                                            {

                                                // Sub settings, if any (might be useful for plugins)
                                                foreach (XmlNode sub in node.ChildNodes)
                                                {
                                                    if (sub.Name != "#text" && sub.InnerText.Trim() != ""
                                                    && (config.NickServPassword != "" ? !sub.InnerText.Contains(config.NickServPassword) : true)
                                                    && (config.ServerPassword != "" ? !sub.InnerText.Contains(config.ServerPassword) : true)
                                                    && search.Match(node.Name).Success
                                                    )
                                                        Functions.Notice(msg.SourceNickname, node.Name + "." + sub.Name + " = " + sub.InnerText);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "topic")
                                {
                                    if (cmd.Length > 4)
                                    {
                                        cmd[4] = cmd[4] == "reset" ? "" : cmd[4]; // !topic reset = set topic to ""

                                        // Set topic if is owner
                                        if (Functions.IsOwner(msg.SourceHostmask))
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "topic (set topic)");
                                            Functions.WriteData("TOPIC " + msg.Target + " :" + string.Join(" ", cmd.Skip(4).ToArray()));
                                            Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": Topic has been set.");
                                        }
                                        else
                                        {
                                            Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "topic (set topic).");
                                            Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                        }
                                    }
                                    else
                                    {
                                        Functions.WriteData("TOPIC " + msg.Target);

                                        bool foundTopic = false;
                                        string topic = "";
                                        while (!foundTopic)
                                        {
                                            topic = reader.ReadLine();
                                            if (DebuggingEnabled == true)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Functions.Log(topic);
                                                Console.ResetColor();
                                            }
                                            if (topic.Contains("331"))
                                            {
                                                topic = "No topic is set for this channel.";
                                                foundTopic = true;
                                            }
                                            else if (topic.Contains("332"))
                                            {
                                                topic = "The topic is: " + string.Join(":", topic.Split(':').Skip(2).ToArray());
                                                foundTopic = true;
                                            }
                                        }
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": " + topic);
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "topic (read topic).");
                                    }
                                }
                                else if (cmd[3] == ":GTFO")
                                {
                                    if (cmd.Length > 4)
                                    {
                                        if (Functions.IsOwner(msg.SourceHostmask))
                                        {
                                            Functions.Log(msg.SourceNickname + " told " + cmd[4] + " to GTFO of " + msg.Target + ", so I kicked " + cmd[4]);
                                            Functions.WriteData("KICK " + msg.Target + " " + cmd[4] + " :GTFO!");
                                        }
                                        else
                                        {
                                            Functions.Log(msg.SourceNickname + " told " + cmd[4] + " to GTFO of " + msg.Target + ", so I kicked " + msg.SourceNickname + " for being mean.");
                                            Functions.WriteData("KICK " + msg.Target + " " + msg.SourceNickname + " :NO U");
                                        }
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "kicklines")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        if (cmd.Length > 4)
                                        {
                                            if (cmd[4].Equals("add") && cmd.Length > 5)
                                            {
                                                string theline = string.Join(" ", cmd.Skip(5).ToArray());
                                                if (File.Exists("Kicks.txt"))
                                                {
                                                    List<string> kicklist = new List<string>();
                                                    kicklist.Add(theline);
                                                    kicklist.AddRange(File.ReadAllLines("Kicks.txt"));
                                                    //string[] text = { string.Join(" ", cmd.Skip(5).ToArray() + "\r\n") + " " + string.Join(" ", pretext.Skip(0).ToArray()) + "\r\n" };
                                                    File.WriteAllLines("Kicks.txt", kicklist.ToArray());
                                                    kicklist = null;
                                                }
                                                else
                                                    File.WriteAllText("Kicks.txt", theline); // could it be more simple?
                                                Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": Done. Added line " + IrcFormatting.BoldText(string.Join(" ", cmd.Skip(5).ToArray())) + " to kicks database.");
                                            }
                                            if (cmd[4].Equals("clear"))
                                            {
                                                if (File.Exists("Kicks.txt"))
                                                {
                                                    File.Delete("Kicks.txt");
                                                    Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": Done. Deleted kicks database.");
                                                }
                                                else Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": Kicks database already deleted.");
                                            }
                                            if (cmd[4].Equals("total") && File.Exists("Kicks.txt"))
                                            {
                                                int i = 0;
                                                string line;
                                                System.IO.StreamReader file = new System.IO.StreamReader("Kicks.txt");
                                                while ((line = file.ReadLine()) != null)
                                                {
                                                    i++;
                                                }
                                                file.Close();
                                                Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": " + i + " lines.");
                                            }
                                            if (cmd[4].Equals("read") && cmd.Length > 5)
                                            {
                                                if (File.Exists("Kicks.txt"))
                                                {
                                                    int i = 0;
                                                    int x;
                                                    string line;
                                                    if (!int.TryParse(cmd[5], out x))
                                                    {
                                                        Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": This isn't a valid number.");
                                                    }
                                                    else
                                                    {
                                                        x--;
                                                        if (x < 0)
                                                        {
                                                            Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": This isn't a valid number.");
                                                        }
                                                        else
                                                        {
                                                            System.IO.StreamReader file = new System.IO.StreamReader("Kicks.txt");
                                                            while ((line = file.ReadLine()) != null && i != x)
                                                            {
                                                                i++;
                                                            }
                                                            if (i == x)
                                                            {
                                                                if (line != null)
                                                                {
                                                                    Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": " + line);
                                                                }
                                                                else
                                                                {
                                                                    Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": No kickline for this number.");
                                                                }
                                                            }
                                                            file.Close();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Functions.WriteData("PRIVMSG " + cmd[2] + " :" + msg.SourceNickname + ": There is no kicks database!");
                                                }
                                            }
                                            string[] command = cmd[3].Split(':');
                                            Functions.Log(msg.SourceNickname + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                        }
                                    }
                                    else
                                    {
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                        string[] command = cmd[3].Split(':');
                                        Functions.Log(msg.SourceNickname + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "kick")
                                {
                                    if (cmd.Length > 4)
                                    {
                                        Botop check = new Botop();
                                        if (Functions.IsOwner(msg.SourceHostmask) | check.isBotOp(msg.SourceNickname))
                                        {
                                            if (cmd.Length > 5)
                                            {
                                                Functions.WriteData("KICK " + msg.Target + " " + cmd[4] + " :" + string.Join(" ", cmd.Skip(5).ToArray()));
                                                Functions.Log(msg.SourceNickname + " issued " + prefix + "kick " + cmd[4] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                            }
                                            else if (File.Exists("Kicks.txt"))
                                            {
                                                string[] lines = File.ReadAllLines("Kicks.txt");
                                                Random rand = new Random();
                                                Functions.WriteData("KICK " + msg.Target + " " + cmd[4] + " :" + lines[rand.Next(lines.Length)]);
                                                Functions.Log(msg.SourceNickname + " issued " + prefix + "kick " + cmd[4]);
                                            }
                                            else
                                            {
                                                Functions.WriteData("KICK " + msg.Target + " " + cmd[4] + " :Goodbye! You just got kicked by " + msg.SourceNickname + ".");
                                                Functions.Log(msg.SourceNickname + " issued " + prefix + "kick " + cmd[4]);
                                            }
                                            //  Functions.WriteData("KICK " + msg.Target + " " + cmd[4] + " Gotcha! You just got ass-kicked by " + nick + "."); // might also be an idea ;D
                                        }
                                        else
                                        {
                                            Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                            Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "kick " + cmd[4]);
                                        }
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "join")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask) && cmd.Length > 4)
                                    {
                                        Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": Joining " + cmd[4] + "...");
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "join " + cmd[4]);
                                        Functions.WriteData("JOIN " + cmd[4]);
                                    }
                                    else if (!Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "join " + cmd[4]);
                                        Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "help")
                                {
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "help");
                                    Thread HelpThread = new Thread(new ParameterizedThreadStart(Functions.SendHelp));
                                    HelpThread.IsBackground = true;
                                    string[] param = { msg.SourceNickname, msg.SourceHostmask, prefix };
                                    HelpThread.Start(param);
                                }
                                else if (cmd[3] == ":" + prefix + "mode")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        if (cmd.Length > 5)
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + msg.Target);
                                            Functions.WriteData("MODE " + msg.Target + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                                        }
                                        else if (cmd.Length > 4)
                                        {
                                            Functions.Log(msg.SourceNickname + " issued " + prefix + "mode " + cmd[4] + " on " + msg.Target);
                                            Functions.WriteData("MODE " + msg.Target + " " + cmd[4]);
                                        }
                                    }
                                    else if (!Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        if (cmd.Length > 5)
                                        {
                                            Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + msg.Target);
                                            Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                        }
                                        else if (cmd.Length > 4)
                                        {
                                            Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "mode " + cmd[4] + " on " + msg.Target);
                                            Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                        }
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "part")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask) && cmd.Length > 4)
                                    {
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                        if (cmd.Length > 5)
                                            cmd[5] = ":" + cmd[5];
                                        Functions.WriteData("PART " + string.Join(" ", cmd.Skip(4).ToArray()));
                                    }
                                    else if (!Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                        Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "reset")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        FileInfo fi = new FileInfo("CSharpBot.xml");
                                        fi.Delete();
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "reset");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": Configuration reset. The bot will now restart.");
                                        Functions.Quit("Resetting!");
                                        ProgramRestart = true;
                                        goto start;
                                    }
                                    else
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "reset");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " :" + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "action")
                                {
                                    Functions.Action(msg.Target, string.Join(" ", cmd.Skip(4).ToArray()));
                                }
                                else if (cmd[3] == ":" + prefix + "restart")
                                {
                                    if (Functions.IsOwner(msg.SourceHostmask))
                                    {
                                        Functions.Log(msg.SourceNickname + " issued " + prefix + "restart");
                                        Functions.Quit("Restarting!");
                                        ProgramRestart = true;
                                        goto start;
                                    }
                                    else
                                    {
                                        Functions.Log(msg.SourceNickname + " attempted to use " + prefix + "restart");
                                        Functions.WriteData("PRIVMSG " + msg.Target + " : " + msg.SourceNickname + ": You are not my owner!");
                                    }
                                }
                                else if (cmd[3] == ":" + prefix + "hostmask" && cmd.Length > 4)
                                {
                                    /*
                                    whoiscaller = msg.SourceNickname;
                                    whoistarget = cmd[4];
                                    whoischan = cmd[2];
                                    currentcmd = "whois";
                                    Functions.WriteData("WHOIS " + cmd[4]);
                                     */
                                    Functions.Log(msg.SourceNickname + " issued " + prefix + "hostmask " + cmd[4]);
                                    whoistarget = cmd[4];
                                    inputline = reader.ReadLine();
                                    cmd = inputline.Split(' ');
                                    IrcNumericReplyLine reply = new IrcNumericReplyLine(inputline);
                                    if (reply.ReplyCode == IrcReplyCode.RPL_WHOISUSER)
                                    {
                                        // WHOIS reply
                                        if (cmd.Length > 6)
                                        {
#if DEBUG
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Functions.Log("Reading WHOIS to get hostmask of " + whoistarget + " for " + msg.SourceNickname + "...");
                                            Console.ForegroundColor = ConsoleColor.White;
#endif
                                            Functions.PrivateMessage(msg.Target, msg.SourceNickname + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                                            Functions.Log("Found the hostmask that " + msg.SourceNickname + " called for, of " + whoistarget + "'s hostmask, which is: " + cmd[5]);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region NOTICE
                        else if (msg.MessageType == IrcMessageType.Notice)
                        {

                            if (msg.SourceNickname == "NickServ")
                                Functions.Log("NickServ info: " + msg.Message);

                            else
                            {
                                if (msg.SourceNickname == "NickServ")
                                    Functions.Log("NickServ identification: " + string.Join(" ", cmd.Skip(3)).Substring(1));
                                else
                                    Functions.WriteData("NOTICE " + msg.SourceNickname + " :Sorry, but you need to contact me over a channel.");
                            }
                        }
                        #endregion
                        #region CTCP Request
                        else
                            if (msg.MessageType == IrcMessageType.CtcpRequest)
                            {
                                Functions.Log("CTCP by " + msg.SourceNickname + ".");
                                // CTCP request

                                string[] spl = msg.Message.Split(' ');
                                string ctcpCmd = spl[0];
                                string[] ctcpParams = spl.Skip(1).ToArray();

                                if (ctcpCmd == "VERSION")
                                {
                                    if (DebuggingEnabled == true)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Functions.Log("Sent CTCP VERSION reply to " + msg.SourceNickname + ".");
                                    }
                                    Functions.WriteData("NOTICE " + msg.SourceNickname + " :\x01VERSION MerbosMagic CSharpBot " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\x01");
                                }
                                else if (ctcpCmd == "PING")
                                {
                                    if (DebuggingEnabled == true)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Functions.Log("Sent CTCP PING reply to " + msg.SourceNickname + ".");
                                    }
                                    if (ctcpParams.Length == 0) ctcpParams = new string[] {
                                    Convert.ToString(DateTime.UtcNow.ToBinary(), 16)
                                };
                                    Functions.WriteData("NOTICE " + msg.SourceNickname + " :\x01PING " + string.Join(" ", ctcpParams) + "\x01");
                                }
                            }
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                Functions.WriteData("PRIVMSG " + CHANNEL + " : Error! Error: " + e.ToString());
                Functions.WriteData("PRIVMSG " + CHANNEL + " : Error! StackTrace: " + e.StackTrace);
                Functions.Quit("Exception: " + e.ToString());

                Console.ForegroundColor = ConsoleColor.Red;
                Functions.Log("The bot generated an error:");
                Functions.Log(e.ToString());
                Functions.Log("Restarting in 5 seconds...");
                Console.ResetColor();

                Thread.Sleep(5000);

                ProgramRestart = true;
                goto start; // restart
                //Environment.Exit(0); // you might also use return
            }
        }

        public void Join()
        {

        }

        void CSharpBot_Kicked(CSharpBot bot, string source, string channel)
        {
            if (bot.RejoinOnKick)
                bot.Functions.Join(channel);
        }

        void CSharpBot_NumericReplyReceived(CSharpBot bot, IrcNumericReplyLine reply)
        {
            if (reply.ReplyCode == IrcReplyCode.ERR_NEEDTOBEREGISTERED) // Error code from GeekShed IRC Network
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Functions.Log("Private messaging is not available, since we need to identify ourself successfully.");
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (reply.ReplyCode == IrcReplyCode.RPL_ENDOFMOTD)
            {
                if (DebuggingEnabled)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Functions.Log("MOTD received.");
                    Console.ResetColor();
                }
                Functions.Log("Applying optimal flags...");
                Functions.WriteData("MODE " + NICK + " +B"); // We are a bot
                Functions.WriteData("MODE " + NICK + " +w"); // We want to get wallops, if any
                Functions.WriteData("MODE " + NICK + " -i"); // We don't want to be invisible
                Functions.Log("Joining " + CHANNEL + "...");
                Functions.WriteData("JOIN " + CHANNEL);
                if (config.ServerPassword != "")
                    Functions.Pass(config.ServerPassword);
                if (config.NickServPassword != "")
                {
                    Functions.Log("Identifying through NickServ...");
                    if (config.NickServAccount != "")
                    {
                        Functions.PrivateMessage("NickServ", "IDENTIFY " + config.NickServAccount + " " + config.NickServPassword);
                    }
                    else
                    {
                        Functions.PrivateMessage("NickServ", "IDENTIFY " + config.NickServPassword);
                    }
                }
            }
        }
    }
}