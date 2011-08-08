using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

namespace CSharpBot
{
    class Program
    {
        // Our RegEx'es
        public static Regex HostmaskRegex;

        public static StreamWriter writer;

        public static bool DEBUG = false;

        #region XML implementation
        public static XmlConfiguration config;
        public static string XmlFileName = "CSharpBot.xml";

        // Originally, configuration was saved in plain text.
        // For our comfort, I'll just dynamically replace the old things with the XML settings.
        static string NICK
        {
            get { return config.Nickname; }
            set { config.Nickname = value; }
        }
        static string CHANNEL
        {
            get { return config.Channel; }
            set { config.Channel = value; }
        }
        static string prefix
        {
            get { return config.Prefix; }
            set { config.Prefix = value; }
        }
        static string SERVER
        {
            get { return config.Server; }
            set { config.Server = value; }
        }
        public static bool logging
        {
            get { return config.EnableFileLogging; }
            set { config.EnableFileLogging = value; }
        }
        #endregion   

        #region IRC formatting

        const string IRCBold = "\x02"; // \x02[text]\x02
        const string IRCColor = "\x03"; // \x03[xx[,xx]]
        const string IRCItalic = "\u0016"; // Mibbit has a bug on this
        const string IRCReset = "\u000F"; // Resets text formatting

        public static string BoldText(string text) { return IRCBold + text + IRCBold; }
        public static string ItalicText(string text) { return IRCItalic + text + IRCItalic; }
        public static string ColorText(string text, int foreground, int background = -1)
        {
            return IRCColor + (foreground < 10 ? "0" : "") + foreground.ToString() + (background > -1 ? (background < 10 ? "0" : "") + background.ToString() : "") + text + IRCColor + "99";
        }
        #endregion

        public static DateTime startupTime = DateTime.Now;
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

        static void Main(string[] args)
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
                        DEBUG = true;
                        break;
                    case "-f":
                    case "--config-file":
                        XmlFileName = value;
                        break;
                }
            }

            bool wentto = false;
        start: // This is the point at which the bot restarts on errors
            if (wentto == true)
            {
                Console.WriteLine("");
                HostmaskRegex = null;
                wentto = false;
            }

            NetworkStream stream;
            TcpClient irc;
            StreamReader reader;
            config = new XmlConfiguration();

            string inputline;

            // Head-lines
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("CSharpBot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("\t(c) Merbo August 3, 2011-Present");
            Console.WriteLine("\t(c) Icedream August 5, 2011-Present");
            Console.WriteLine("\t(c) peapodamus August 7 2011-Present");
            Console.WriteLine();

            // First setup
            if (!File.Exists(XmlFileName))
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
                        if (DEBUG == true) 
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
                    if (DEBUG == true)
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
            try
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Functions.Log("Connecting to " + config.Server + "...");

                irc = new TcpClient(config.Server, config.Port);
                if (!irc.Connected)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Functions.Log("Connection failed. Bot is restarting in 5 seconds...");
                    System.Threading.Thread.Sleep(5000);
                    wentto = true;
                    goto start;
                }

                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                Functions.Log("Logging in...");
                Functions.WriteData(config.Userline);
                Functions.WriteData("NICK " + config.Nickname);
                if (config.ServerPassword != "")
                    Functions.WriteData("PASS " + config.ServerPassword);
                if (config.NickServPassword != "")
                {
                    Functions.Log("Identifying through NickServ...");
                    if (config.NickServAccount == "")
                    {
                        Functions.WriteData("PRIVMSG NickServ :IDENTIFY " + config.NickServPassword);
                    }
                    else
                    {
                        Functions.WriteData("PRIVMSG NickServ :IDENTIFY " + config.NickServAccount + " " + config.NickServPassword);
                    }
                }
                string currentcmd = null;
                string whoiscaller = null;
                string whoistarget = null;
                string whoischan = null;
                while ((inputline = reader.ReadLine()) != null)
                {
                    string[] cmd = inputline.Split(' ');
                    if (DEBUG == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (!cmd[0].Equals("PING"))
                            Functions.Log("RECV: " + inputline);
                        Console.ResetColor();
                    }
                    if (cmd[0].Equals("PING"))
                    {
                        if (DEBUG == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Functions.Log("Ping? Pong!");
                            Console.ResetColor();
                        }

                        Functions.WriteData("PONG " + cmd[1]);
                    }
                    if (cmd[1].Equals("486")) // Error code from GeekShed IRC Network
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Functions.Log("Private messaging is not available, since we need to identify ourself successfully.");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if (cmd[1].Equals("376"))
                    {
                        if (DEBUG == true)
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
                    }
                    else if (cmd[1].Equals("311"))
                    {
                        if (currentcmd.Equals("Whois") && cmd.Length > 6)
                        {
                            Functions.Log("Reading WHOIS to get hostmask of " + whoistarget + " for " + whoiscaller + "...");
                            Functions.WriteData("PRIVMSG " + whoischan + " :" + whoiscaller + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                            Functions.Log("Found the hostmask that " + whoiscaller + " called for, of " + whoistarget + "'s hostmask, which is: " + cmd[5]);
                        }
                    }
                    else if (cmd[1].Equals("KICK") && cmd[3] == NICK)
                    {
                        Functions.Log("Rejoining " + cmd[2]);
                        Functions.WriteData("JOIN " + cmd[2]);
                    }

                    else if (cmd[1].Equals("NOTICE"))
                    {
                        // Splitting it up
                        string[] prenick1 = cmd[0].Split(':');
                        string[] prenick = prenick1[1].Split('!');
                        string nick = prenick[0];
                        string[] preident = { };
                        string ident = nick;
                        string host = SERVER;
                        string target = NICK;
                        string message = string.Join(" ", cmd.Skip(3).ToArray()).Substring(1);

                        try
                        {
                            preident = prenick[1].Split('@');
                            ident = preident[0];
                            host = preident[1];
                            target = cmd[2];
                        }
                        catch (Exception)
                        {
                            // Do nothing; leave this line, since it prevents errors during runtim
                        }
                        finally
                        {
                            if (nick == "NickServ")
                                Functions.Log("NickServ info: " + message);
                        }
                    }

                    // Someone sent a valid command
                    else if (cmd[1].Equals("PRIVMSG"))
                    {
                        // Splitting it up
                        string[] prenick1 = cmd[0].Split(':');
                        string[] prenick = prenick1[1].Split('!');
                        string nick = prenick[0];
                        string[] preident = prenick[1].Split('@');
                        string ident = preident[0];
                        string host = preident[1];
                        string chan = cmd[2];
                        if (chan.StartsWith("#"))
                        // Source is really a channel
                        {

                            // Execute commands
                            if (cmd[3] == ":" + prefix + "test")
                            {
                                Functions.Log(nick + " issued " + prefix + "test");
                                Functions.WriteData("PRIVMSG " + chan + " :I think your test works ;-)");
                            }
                            else if (cmd[3] == ":" + prefix + "amiowner")
                            {
                                Functions.Log(nick + " issued " + prefix + "amiowner");
                                Functions.WriteData("PRIVMSG " + chan + " :The answer is: " + (Functions.IsOwner(prenick1[1]) ? "Yes!" : "No!"));
                            }
                            else if (cmd[3] == ":" + prefix + "uptime")
                            {
                                TimeSpan ts = DateTime.Now - startupTime;
                                //   string ut = "I have been running for " + ts.TotalMinutes + " Minutes";
                                string ut = ts.ToString();
                                Functions.Say(chan, ut);
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
                                    Functions.Log(nick + " issued " + prefix + "time " + cmd[4]);
                                else
                                    Functions.Log(nick + " issued " + prefix + "time");
                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": It's " + DateTime.UtcNow.AddHours(add).ToString() + "(UTC" + adds + ")");
                            }
                            else if (cmd[3] == ":" + prefix + "mynick")
                            {
                                Functions.Log(nick + " issued " + prefix + "mynick");
                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Your nick is " + nick);
                            }
                            else if (cmd[3] == ":" + prefix + "myident")
                            {
                                Functions.Log(nick + " issued " + prefix + "myident");
                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Your ident is " + ident);
                            }
                            else if (cmd[3] == ":" + prefix + "myhost")
                            {
                                Functions.Log(nick + " issued " + prefix + "myhost");
                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Your host is " + host);
                            }
                            else if (cmd[3] == ":" + prefix + "myfullmask")
                            {
                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Your full mask is " + cmd[0]);
                            }
                            else if (cmd[3] == ":" + prefix + "die")
                            {
                                if (Functions.IsOwner(prenick1[1]))
                                {
                                    if (cmd.Length > 4)
                                    {
                                        Functions.Log(nick + " issued " + prefix + "die " + string.Join(" ", cmd.Skip(5).ToArray()));
                                        Functions.WriteData("QUIT :" + string.Join(" ", cmd.Skip(5).ToArray()));
                                    }
                                    else
                                    {
                                        Functions.Log(nick + " issued " + prefix + "die");
                                        Functions.WriteData("QUIT :I shot myself because " + nick + " told me to.");
                                    }
                                }
                                else
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "die");
                                    Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "clean")
                            {
                                if (Functions.IsOwner(prenick1[1]))
                                {
                                    FileInfo fi = new FileInfo("options.txt");
                                    fi.Delete();
                                    Functions.Log(nick + " issued " + prefix + "clean");
                                    Functions.WriteData("QUIT :Cleaned!");
                                }
                                else
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "clean");
                                    Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                }
                            }

                            else if (cmd[3] == ":" + prefix + "topic")
                            {
                                if (cmd.Length > 4)
                                {
                                    cmd[4] = cmd[4] == "reset" ? "" : cmd[4]; // !topic reset = set topic to ""

                                    // Set topic if is owner
                                    if (Functions.IsOwner(prenick1[1]))
                                    {
                                        Functions.Log(nick + " issued " + prefix + "topic (set topic)");
                                        Functions.WriteData("TOPIC " + chan + " :" + string.Join(" ", cmd.Skip(4).ToArray()));
                                        Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Topic has been set.");
                                    }
                                    else
                                    {
                                        Functions.Log(nick + " attempted to use " + prefix + "topic (set topic).");
                                        Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                    }
                                }
                                else
                                {
                                    Functions.WriteData("TOPIC " + chan);

                                    bool foundTopic = false;
                                    string topic = "";
                                    while (!foundTopic)
                                    {
                                        topic = reader.ReadLine();
                                        if (DEBUG == true)
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
                                    Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": " + topic);
                                    Functions.Log(nick + " issued " + prefix + "topic (read topic).");
                                }
                            }
                            else if (cmd[3] == ":GTFO")
                            {
                                if (cmd.Length > 4)
                                {
                                    if (Functions.IsOwner(prenick1[1]))
                                    {
                                        Functions.Log(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + cmd[4]);
                                        Functions.WriteData("KICK " + chan + " " + cmd[4] + " :GTFO!");
                                    }
                                    else
                                    {
                                        Functions.Log(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + nick + " for being mean.");
                                        Functions.WriteData("KICK " + chan + " " + nick + " :NO U");
                                    }
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "kicklines")
                            {
                                if (Functions.IsOwner(prenick1[1]))
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
                                            Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Done. Added line " + IRCBold + string.Join(" ", cmd.Skip(5).ToArray()) + IRCBold + " to kicks database.");
                                        }
                                        if (cmd[4].Equals("clear"))
                                        {
                                            if (File.Exists("Kicks.txt"))
                                            {
                                                File.Delete("Kicks.txt");
                                                Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Done. Deleted kicks database.");
                                            }
                                            else Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": Kicks database already deleted.");
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
                                            Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": " + i + " lines.");
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
                                                    Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": This isn't a valid number.");
                                                }
                                                else
                                                {
                                                    x--;
                                                    if (x < 0)
                                                    {
                                                        Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": This isn't a valid number.");
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
                                                                Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": " + line);
                                                            }
                                                            else
                                                            {
                                                                Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": No kickline for this number.");
                                                            }
                                                        }
                                                        file.Close();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Functions.WriteData("PRIVMSG " + cmd[2] + " :" + nick + ": There is no kicks database!");
                                            }
                                        }
                                        string[] command = cmd[3].Split(':');
                                        Functions.Log(nick + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                    }
                                }
                                else
                                {
                                    Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                    string[] command = cmd[3].Split(':');
                                    Functions.Log(nick + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "kick")
                            {
                                if (cmd.Length > 4)
                                {
                                    if (Functions.IsOwner(prenick1[1]))
                                    {
                                        if (cmd.Length > 5)
                                        {
                                            Functions.WriteData("KICK " + chan + " " + cmd[4] + " :" + string.Join(" ", cmd.Skip(5).ToArray()));
                                            Functions.Log(nick + " issued " + prefix + "kick " + cmd[4] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                        }
                                        else if (File.Exists("Kicks.txt"))
                                        {
                                            string[] lines = File.ReadAllLines("Kicks.txt");
                                            Random rand = new Random();
                                            Functions.WriteData("KICK " + chan + " " + cmd[4] + " :" + lines[rand.Next(lines.Length)]);
                                            Functions.Log(nick + " issued " + prefix + "kick " + cmd[4]);
                                        }
                                        else
                                        {
                                            Functions.WriteData("KICK " + chan + " " + cmd[4] + " :Goodbye! You just got kicked by " + nick + ".");
                                            Functions.Log(nick + " issued " + prefix + "kick " + cmd[4]);
                                        }
                                        //  Functions.WriteData("KICK " + chan + " " + cmd[4] + " Gotcha! You just got ass-kicked by " + nick + "."); // might also be an idea ;D
                                    }
                                    else
                                    {
                                        Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                        Functions.Log(nick + " attempted to use " + prefix + "kick " + cmd[4]);
                                    }
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "join")
                            {
                                if (Functions.IsOwner(prenick1[1]) && cmd.Length > 4)
                                {
                                    Functions.Log(nick + " issued " + prefix + "join " + cmd[4]);
                                    Functions.WriteData("JOIN " + cmd[4]);
                                }
                                else if (!Functions.IsOwner(prenick1[1]))
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "join " + cmd[4]);
                                    Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "help")
                            {
                                Functions.Log(nick + " issued " + prefix + "help");
                                Thread HelpThread = new Thread(new ParameterizedThreadStart(Functions.SendHelp));
                                HelpThread.IsBackground = true;
                                string[] param = { nick, prefix };
                                HelpThread.Start(param);
                            }
                            else if (cmd[3] == ":" + prefix + "mode")
                            {
                                if (Functions.IsOwner(prenick1[1]))
                                {
                                    if (cmd.Length > 5)
                                    {
                                        Functions.Log(nick + " issued " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                        Functions.WriteData("MODE " + chan + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                                    }
                                    else if (cmd.Length > 4)
                                    {
                                        Functions.Log(nick + " issued " + prefix + "mode " + cmd[4] + " on " + chan);
                                        Functions.WriteData("MODE " + chan + " " + cmd[4]);
                                    }
                                }
                                else if (!Functions.IsOwner(prenick1[1]))
                                {
                                    if (cmd.Length > 5)
                                    {
                                        Functions.Log(nick + " attempted to use " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                        Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                    }
                                    else if (cmd.Length > 4)
                                    {
                                        Functions.Log(nick + " attempted to use " + prefix + "mode " + cmd[4] + " on " + chan);
                                        Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                    }
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "part")
                            {
                                if (Functions.IsOwner(prenick1[1]) && cmd.Length > 4)
                                {
                                    Functions.Log(nick + " issued " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                    if (cmd.Length > 5)
                                        cmd[5] = ":" + cmd[5];
                                    Functions.WriteData("PART " + string.Join(" ", cmd.Skip(4).ToArray()));
                                }
                                else if (!Functions.IsOwner(prenick1[1]))
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                    Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "reset")
                            {
                                if (Functions.IsOwner(prenick1[1]))
                                {
                                    FileInfo fi = new FileInfo("options.txt");
                                    fi.Delete();
                                    Functions.Log(nick + " issued " + prefix + "reset");
                                    Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": Configuration reset. The bot will now restart.");
                                    Functions.WriteData("QUIT :Resetting!");
                                    wentto = true;
                                    goto start;
                                }
                                else
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "reset");
                                    Functions.WriteData("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "restart")
                            {
                                if (Functions.IsOwner(prenick1[1]))
                                {
                                    Functions.Log(nick + " issued " + prefix + "restart");
                                    Functions.WriteData("QUIT :Restarting!");
                                    wentto = true;
                                    goto start;
                                }
                                else
                                {
                                    Functions.Log(nick + " attempted to use " + prefix + "restart");
                                    Functions.WriteData("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                }
                            }
                            else if (cmd[3] == ":" + prefix + "hostmask" && cmd.Length > 4)
                            {
                                whoiscaller = nick;
                                whoistarget = cmd[4];
                                whoischan = cmd[2];
                                currentcmd = "Whois";
                                Functions.WriteData("WHOIS " + cmd[4]);
                                Functions.Log(nick + " issued " + prefix + "hostmask " + cmd[4]);
                            }
                            else if (cmd[3].StartsWith(":" + prefix + "math "))
                            {
                                string mathToParse = cmd[3].Substring(cmd[3].IndexOf(prefix));
                                Functions.WriteData("PRIVMSG " + chan + " : " + Math.Math.Parse(mathToParse));
                            }
                        }
                        else
                        {
                            string message = string.Join(" ", cmd.Skip(3)).Substring(1);
                            if (message.StartsWith("\x01") && message.EndsWith("\x01"))
                            {
                                Functions.Log("CTCP by " + nick + ".");
                                // CTCP request
                                message = message.Trim('\x01');

                                string[] spl = message.Split(' ');
                                string ctcpCmd = spl[0];
                                string[] ctcpParams = spl.Skip(1).ToArray();

                                if (ctcpCmd == "VERSION")
                                {
                                    if (DEBUG == true)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Functions.Log("Sent CTCP VERSION reply to " + nick + ".");
                                    }
                                    Functions.WriteData("NOTICE " + nick + " :\x01VERSION MerbosMagic CSharpBot " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\x01");
                                }
                                else if (ctcpCmd == "PING")
                                {
                                    if (DEBUG == true)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Functions.Log("Sent CTCP PING reply to " + nick + ".");
                                    }
                                    if (ctcpParams.Length == 0) ctcpParams = new string[] {
                                    Convert.ToString(DateTime.UtcNow.ToBinary(), 16)
                                };
                                    Functions.WriteData("NOTICE " + nick + " :\x01PING " + string.Join(" ", ctcpParams) + "\x01");
                                }
                            }
                            else
                            {
                                Functions.Log("Private message by " + nick + ": " + message);
                                if (nick == "NickServ")
                                    Functions.Log("NickServ identification: " + string.Join(" ", cmd.Skip(3)).Substring(1));
                                else
                                    Functions.WriteData("NOTICE " + nick + " :Sorry, but you need to contact me over a channel.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Functions.WriteData("PRIVMSG " + CHANNEL + " : Error! Error: " + e.ToString());
                Functions.WriteData("PRIVMSG " + CHANNEL + " : Error! StackTrace: " + e.StackTrace);
                Functions.WriteData("QUIT :Exception: " + e.ToString());

                Console.ForegroundColor = ConsoleColor.Red;
                Functions.Log("The bot generated an error:");
                Functions.Log(e.ToString());
                Functions.Log("Restarting in 5 seconds...");
                Console.ResetColor();

                Thread.Sleep(5000);

                wentto = true;
                goto start; // restart
                //Environment.Exit(0); // you might also use return
            }
        }
    }
}