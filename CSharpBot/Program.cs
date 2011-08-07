using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

class Program
{
    // Our RegEx'es
    public static Regex HostmaskRegex;

    public static StreamWriter writer;

    const string IRCBold = "\x02"; // \x02[text]\x02
    const string IRCColor = "\x03"; // \x03[xx[,xx]]
    const string IRCItalic = "\u0016"; // Mibbit has a bug on this
    const string IRCReset = "\u000F"; // Resets text formatting

    public static bool logging;

    public string BoldText(string text) { return IRCBold + text + IRCBold; }
    public string ItalicText(string text) { return IRCItalic + text + IRCItalic; }
    public string ColorText(string text, int foreground, int background = -1)
    {
        return IRCColor + (foreground < 10 ? "0" : "") + foreground.ToString() + (background > -1 ? (background < 10 ? "0" : "") + background.ToString() : "") + text + IRCColor + "99";
    }

    static void Main(string[] args)
    {
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

        string inputline;
        string prefix = "";
        string ownerhost = "";
        string CHANNEL = "";
        string NICK;
        string SERVER;
        string USER;
        // For identification (NickServ...)
        string serverpassword = "";
        string nickservpassword = "";
        int PORT = -1;

        // Head-lines
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("CSharpBot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        Console.WriteLine("\t(c) Merbo August 3, 2011-Present");
        Console.WriteLine("\t(c) Icedream August 5, 2011-Present");
        Console.WriteLine();

        // First setup
        if (!File.Exists("Options.txt"))
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
            SERVER = Console.ReadLine();

            // The PORT
            Console.ForegroundColor = ConsoleColor.Cyan;
            while (PORT < 0 || PORT > 0xffff)
            {
                Console.Write("Port: ");

                // Errors?
                Console.ForegroundColor = ConsoleColor.White;
                bool validNumber = int.TryParse(Console.ReadLine(), out PORT);
                Console.ForegroundColor = ConsoleColor.Red;
                if (!validNumber)
                    Console.WriteLine("Sorry, but this is an invalid port number!");
                if (PORT > 0xffff)
                    Console.WriteLine("Sorry, but this is a too big number.");
                if (PORT < 0)
                    Console.WriteLine("Sorry, but this is a too small number.");
            }

            // The NICKname
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Nick: ");
            Console.ForegroundColor = ConsoleColor.White;
            NICK = Console.ReadLine();

            // The server password
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Server password (optional): ");
            Console.ForegroundColor = ConsoleColor.White;
            serverpassword = Console.ReadLine();

            // The nickserv password
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("NickServ password (optional): ");
            Console.ForegroundColor = ConsoleColor.White;
            nickservpassword = Console.ReadLine();

            // The CHANNEL
            while (!CHANNEL.StartsWith("#"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Channel: ");
                Console.ForegroundColor = ConsoleColor.White;
                CHANNEL = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (!CHANNEL.StartsWith("#"))
                    Console.WriteLine("Sorry, but channel names always begin with #!");
            }

            // The ownerhost
            while (HostmaskRegex == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Hostmask of owner (Nickname!Username@Host, can be a regex globmask (wildcards like * and ?)): ");
                Console.ForegroundColor = ConsoleColor.White;
                ownerhost = Console.ReadLine();
                try
                {
                    HostmaskRegex = new Regex(ownerhost = "^" + ownerhost.Replace(".", "\\.").Replace("*", ".+") + "$");
                    #if DEBUG
                         Console.ForegroundColor = ConsoleColor.Yellow;
                         Console.WriteLine("(debug) Parsed Regex: " + ownerhost);
                         Console.ResetColor();
                    #endif
                }
                catch (Exception n)
                {
                    Log("Something went wrong: " + n.Message);
                    Log("Exception: " + n.ToString());
                    Log("StackTrace: " + n.StackTrace);
                }
            }

            // The prefix
            while (prefix.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Command Prefix (e.g. in '!kick' it is '!'): ");
                Console.ForegroundColor = ConsoleColor.White;
                prefix = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (prefix.Length < 1)
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
                logging = true;
            }
            else if (yn.Key == ConsoleKey.N)
            {
                logging = false;
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
            USER = "USER " + NICK + " 8 * :MerbosMagic C# IRC Bot";
            string[] options = { SERVER, PORT.ToString(), NICK, CHANNEL, ownerhost, prefix, USER, logging.ToString(), serverpassword, nickservpassword };
            try
            {
                File.WriteAllLines("options.txt", options);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration has been saved successfully. The bot will now start!");
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Loading configuration...");
            try
            {
                // Basic options file layout 1.0
                string[] options = File.ReadAllLines("options.txt");
                SERVER = options[0];
                PORT = int.Parse(options[1]);
                NICK = options[2];
                CHANNEL = options[3];
                ownerhost = options[4];
                HostmaskRegex = new Regex(options[4]);
                prefix = options[5];
                USER = options[6];
                logging = (options[7].Equals("True") ? true : false);
                if (options.Length > 8)
                {
                    // Options file layout 1.1
                    serverpassword = options[8];
                    nickservpassword = options[9];
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Your options file's layout may be deprecated. For optimal use, recreate your options file.");
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Configuration has NOT been loaded. Please check if the configuration is valid and try again.");
                Console.WriteLine("Enter something to exit.");
                Console.ReadKey();
                return;
            }
        }
        try
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Log("Connecting to " + SERVER + "...");
            
            irc = new TcpClient(SERVER, PORT);
            if (!irc.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Connection failed. Bot is restarting in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                wentto = true;
                goto start;
            }

            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            Log("Logging in...");
            writer.WriteLine(USER);
            writer.WriteLine("NICK " + NICK);
            if(serverpassword != "")
                writer.WriteLine("PASS " + serverpassword);
            if (nickservpassword != "")
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Yellow;
                Log("Identifying through NickServ...");
#endif
                writer.WriteLine("PRIVMSG NickServ :IDENTIFY " + nickservpassword);
            }
            string currentcmd = null;
            string whoiscaller = null;
            string whoistarget = null;
            string whoischan = null; 
            while ((inputline = reader.ReadLine()) != null)
            {
                string[] cmd = inputline.Split(' ');
                #if DEBUG
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!cmd[0].Equals("PING"))
                        Log("<= " + inputline);
                    Console.ResetColor();
                #endif
                if (cmd[0].Equals("PING")) {
                    #if DEBUG
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Log("Ping? Pong!");
                        Console.ResetColor();
                    #endif
                    writer.WriteLine("PONG " + cmd[1]);
                }
                if (cmd[1].Equals("486")) // Error code from GeekShed IRC Network
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Log("Private messaging is not available, since we need to identify ourself successfully.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (cmd[1].Equals("376"))
                {
                    #if DEBUG
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Log("MOTD received.");
                        Console.ResetColor();
                    #endif
                    Log("Applying optimal flags...");
                    writer.WriteLine("MODE " + NICK + " +B"); // We are a bot
                    writer.WriteLine("MODE " + NICK + " +w"); // We want to get wallops, if any
                    writer.WriteLine("MODE " + NICK + " -i"); // We don't want to be invisible
                    Log("Joining " + CHANNEL + "...");
                    writer.WriteLine("JOIN " + CHANNEL);
                }
                else if (cmd[1].Equals("311"))
                {
                    if (currentcmd.Equals("Whois") && cmd.Length > 6)
                    {
                        Log("Reading WHOIS to get hostmask of " + whoistarget + " for " + whoiscaller + "...");
                        writer.WriteLine("PRIVMSG " + whoischan + " :" + whoiscaller + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                        Log("Found the hostmask that " + whoiscaller + " called for, of " + whoistarget + "'s hostmask, which is: " + cmd[5]);
                    }
                }
                else if (cmd[1].Equals("KICK") && cmd[3] == NICK)
                {
                    Log("Rejoining " + cmd[2]);
                    writer.WriteLine("JOIN " + cmd[2]);
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
                            Log("NickServ info: " + message);
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
                            Log(nick + " issued " + prefix + "test");
                            writer.WriteLine("PRIVMSG " + chan + " :I think your test works ;-)");
                        }
                        else if (cmd[3] == ":" + prefix + "amiowner")
                        {
                            Log(nick + " issued " + prefix + "amiowner");
                            writer.WriteLine("PRIVMSG " + chan + " :The answer is: " + (IsOwner(prenick1[1]) ? "Yes!" : "No!"));
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
                                Log(nick + " issued " + prefix + "time " + cmd[4]);
                            else
                                Log(nick + " issued " + prefix + "time");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": It's " + DateTime.UtcNow.AddHours(add).ToString() + "(UTC" + adds + ")");
                        }
                        else if (cmd[3] == ":" + prefix + "mynick")
                        {
                            Log(nick + " issued " + prefix + "mynick");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your nick is " + nick);
                        }
                        else if (cmd[3] == ":" + prefix + "myident")
                        {
                            Log(nick + " issued " + prefix + "myident");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your ident is " + ident);
                        }
                        else if (cmd[3] == ":" + prefix + "myhost")
                        {
                            Log(nick + " issued " + prefix + "myhost");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your host is " + host);
                        }
                        else if (cmd[3] == ":" + prefix + "myfullmask")
                        {
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your full mask is " + cmd[0]);
                        }
                        else if (cmd[3] == ":" + prefix + "die")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                if (cmd.Length > 4)
                                {
                                    Log(nick + " issued " + prefix + "die " + string.Join(" ", cmd.Skip(5).ToArray()));
                                    writer.WriteLine("QUIT :" + string.Join(" ", cmd.Skip(5).ToArray()));
                                }
                                else
                                {
                                    Log(nick + " issued " + prefix + "die");
                                    writer.WriteLine("QUIT :I shot myself because " + nick + " told me to.");
                                }
                            }
                            else
                            {
                                Log(nick + " attempted to use " + prefix + "die");
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "clean")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                FileInfo fi = new FileInfo("options.txt");
                                fi.Delete();
                                Log(nick + " issued " + prefix + "clean");
                                writer.WriteLine("QUIT :Cleaned!");
                            }
                            else
                            {
                                Log(nick + " attempted to use " + prefix + "clean");
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                            }
                        }

                        else if (cmd[3] == ":" + prefix + "topic")
                        {
                            if (cmd.Length > 4)
                            {
                                cmd[4] = cmd[4] == "reset" ? "" : cmd[4]; // !topic reset = set topic to ""

                                // Set topic if is owner
                                if (IsOwner(prenick1[1]))
                                {
                                    Log(nick + " issued " + prefix + "topic (set topic)");
                                    writer.WriteLine("TOPIC " + chan + " :" + ArrayToString(cmd.Skip(4).ToArray(), " "));
                                    writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Topic has been set.");
                                }
                                else
                                {
                                    Log(nick + " attempted to use " + prefix + "topic (set topic).");
                                    writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                }
                            }
                            else
                            {
                                writer.WriteLine("TOPIC " + chan);

                                bool foundTopic = false;
                                string topic = "";
                                while (!foundTopic)
                                {
                                    topic = reader.ReadLine();
#if DEBUG
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Log(topic);
                                    Console.ResetColor();
#endif
                                    if (topic.Contains("331"))
                                    {
                                        topic = "No topic is set for this channel.";
                                        foundTopic = true;
                                    }
                                    else if (topic.Contains("332"))
                                    {
                                        topic = "The topic is: " + ArrayToString(topic.Split(':').Skip(2).ToArray(), ":");
                                        foundTopic = true;
                                    }
                                }
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": " + topic);
                                Log(nick + " issued " + prefix + "topic (read topic).");
                            }
                        }
                        else if (cmd[3] == ":GTFO")
                        {
                            if (cmd.Length > 4)
                            {
                                if (IsOwner(prenick1[1]))
                                {
                                    Log(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + cmd[4]);
                                    writer.WriteLine("KICK " + chan + " " + cmd[4] + " :GTFO!");
                                }
                                else
                                {
                                    Log(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + nick + " for being mean.");
                                    writer.WriteLine("KICK " + chan + " " + nick + " :NO U");
                                }
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "kicklines")
                        {
                            if (IsOwner(prenick1[1]))
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
                                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Done. Added line " + IRCBold + string.Join(" ", cmd.Skip(5).ToArray()) + IRCBold + " to kicks database.");
                                    }
                                    if (cmd[4].Equals("clear"))
                                    {
                                        if (File.Exists("Kicks.txt"))
                                        {
                                            File.Delete("Kicks.txt");
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Done. Deleted kicks database.");
                                        }
                                        else writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Kicks database already deleted.");
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
                                        writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": " + i + " lines.");
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
                                                writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": This isn't a valid number.");
                                            }
                                            else
                                            {
                                                x--;
                                                if (x < 0)
                                                {
                                                    writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": This isn't a valid number.");
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
                                                            writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": " + line);
                                                        }
                                                        else
                                                        {
                                                            writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": No kickline for this number.");
                                                        }
                                                    }
                                                    file.Close();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            writer.WriteLine("PRIVMSG " + cmd[2] + " :" + nick + ": There is no kicks database!");
                                        }
                                    }
                                    string[] command = cmd[3].Split(':');
                                    Log(nick + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                }
                            }
                            else
                            {
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                                string[] command = cmd[3].Split(':');
                                Log(nick + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "kick")
                        {
                            if (cmd.Length > 4)
                            {
                                if (IsOwner(prenick1[1]))
                                {
                                    if (cmd.Length > 5)
                                    {
                                        writer.WriteLine("KICK " + chan + " " + cmd[4] + " :" + string.Join(" ", cmd.Skip(5).ToArray()));
                                        Log(nick + " issued " + prefix + "kick " + cmd[4] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                                    }
                                    else if (File.Exists("Kicks.txt"))
                                    {
                                        string[] lines = File.ReadAllLines("Kicks.txt");
                                        Random rand = new Random();
                                        writer.WriteLine("KICK " + chan + " " + cmd[4] + " :" + lines[rand.Next(lines.Length)]);
                                        Log(nick + " issued " + prefix + "kick " + cmd[4]);
                                    }
                                    else
                                    {
                                        writer.WriteLine("KICK " + chan + " " + cmd[4] + " :Goodbye! You just got kicked by " + nick + ".");
                                        Log(nick + " issued " + prefix + "kick " + cmd[4]);
                                    }
                                    //  writer.WriteLine("KICK " + chan + " " + cmd[4] + " Gotcha! You just got ass-kicked by " + nick + "."); // might also be an idea ;D
                                }
                                else
                                {
                                    writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                    Log(nick + " attempted to use " + prefix + "kick " + cmd[4]);
                                }
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "join")
                        {
                            if (IsOwner(prenick1[1]) && cmd.Length > 4)
                            {
                                Log(nick + " issued " + prefix + "join " + cmd[4]);
                                writer.WriteLine("JOIN " + cmd[4]);
                            }
                            else if (!IsOwner(prenick1[1]))
                            {
                                Log(nick + " attempted to use " + prefix + "join " + cmd[4]);
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "help")
                        {
                            Log(nick + " issued " + prefix + "help");
                            Thread HelpThread = new Thread(new ParameterizedThreadStart(SendHelp));
                            HelpThread.IsBackground = true;
                            string[] param = { nick, prefix };
                            HelpThread.Start(param);
                        }
                        else if (cmd[3] == ":" + prefix + "mode")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                if (cmd.Length > 5)
                                {
                                    Log(nick + " issued " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                    writer.WriteLine("MODE " + chan + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                                }
                                else if (cmd.Length > 4)
                                {
                                    Log(nick + " issued " + prefix + "mode " + cmd[4] + " on " + chan);
                                    writer.WriteLine("MODE " + chan + " " + cmd[4]);
                                }
                            }
                            else if (!IsOwner(prenick1[1]))
                            {
                                if (cmd.Length > 5)
                                {
                                    Log(nick + " attempted to use " + prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                    writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                }
                                else if (cmd.Length > 4)
                                {
                                    Log(nick + " attempted to use " + prefix + "mode " + cmd[4] + " on " + chan);
                                    writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                }
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "part")
                        {
                            if (IsOwner(prenick1[1]) && cmd.Length > 4)
                            {
                                Log(nick + " issued " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                if (cmd.Length > 5)
                                    cmd[5] = ":" + cmd[5];
                                writer.WriteLine("PART " + string.Join(" ", cmd.Skip(4).ToArray()));
                            }
                            else if (!IsOwner(prenick1[1]))
                            {
                                Log(nick + " attempted to use " + prefix + "part " + string.Join(" ", cmd.Skip(4).ToArray()));
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "reset")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                FileInfo fi = new FileInfo("options.txt");
                                fi.Delete();
                                Log(nick + " issued " + prefix + "reset");
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Configuration reset. The bot will now restart.");
                                writer.WriteLine("QUIT :Resetting!");
                                wentto = true;
                                goto start;
                            }
                            else
                            {
                                Log(nick + " attempted to use " + prefix + "reset");
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "config")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                if (cmd.Length > 4)
                                {
                                    if (cmd[4].Equals("list"))
                                    {
                                        if (File.Exists("options.txt"))
                                        {
                                            string[] options = File.ReadAllLines("options.txt");
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Server: " + options[0]);
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Port: " + options[1]);
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Nick: " + options[2]);
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": AutoJoinChannel: " + options[3]);
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Ownerhost: " + options[4].Replace(".+", "*").Replace("^", "").Replace("$", ""));
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": CommandPrefix: " + options[5]);
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Logging: " + options[7]);
                                        }
                                        else
                                        {
                                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": I'd love to tell you, but there isn't a configuration file :|");
                                        }
                                    }
                                    else if (cmd[4].Equals("edit"))
                                    {
                                        if (cmd.Length > 6)
                                        {
                                            if (File.Exists("options.txt"))
                                            {
                                                string[] options = File.ReadAllLines("options.txt");
                                                System.IO.File.Delete("options.txt");
                                                if (cmd[5].Equals("Server") || cmd[5].Equals("server"))
                                                {
                                                    WriteToFile("options.txt", cmd[6]);
                                                    WriteToFile("options.txt", options[1]);
                                                    WriteToFile("options.txt", options[2]);
                                                    WriteToFile("options.txt", options[3]);
                                                    WriteToFile("options.txt", options[4]);
                                                    WriteToFile("options.txt", options[5]);
                                                    WriteToFile("options.txt", options[6]);
                                                    WriteToFile("options.txt", options[7]);
                                                    Say(chan, "Done. Server is now set to '" + cmd[6] + "'.");
                                                }
                                                else if (cmd[5].Equals("Port") || cmd[5].Equals("port"))
                                                {
                                                    int setport;
                                                    if (int.TryParse(cmd[6], out setport))
                                                    {
                                                        if (setport <= 0xffff && setport >= 0)
                                                        {
                                                            WriteToFile("options.txt", options[0]);
                                                            WriteToFile("options.txt", setport.ToString());
                                                            WriteToFile("options.txt", options[2]);
                                                            WriteToFile("options.txt", options[3]);
                                                            WriteToFile("options.txt", options[4]);
                                                            WriteToFile("options.txt", options[5]);
                                                            WriteToFile("options.txt", options[6]);
                                                            WriteToFile("options.txt", options[7]);
                                                            Say(chan, "Done. port is now set to " + setport + ".");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Say(chan, "Invalid port number!");
                                                    }
                                                }
                                                else if (cmd[5].Equals("Nick") || cmd[5].Equals("nick"))
                                                {
                                                    WriteToFile("options.txt", options[0]);
                                                    WriteToFile("options.txt", options[1]);
                                                    WriteToFile("options.txt", cmd[6]);
                                                    WriteToFile("options.txt", options[3]);
                                                    WriteToFile("options.txt", options[4]);
                                                    WriteToFile("options.txt", options[5]);
                                                    WriteToFile("options.txt", options[6]);
                                                    WriteToFile("options.txt", options[7]);
                                                    Say(chan, "Done. Nickname is now set to " + cmd[6]);
                                                }
                                                else if (cmd[5].Equals("AutoJoinChannel") || cmd[5].Equals("autojoinchannel"))
                                                {
                                                    if (cmd[6].StartsWith("#"))
                                                    {
                                                        WriteToFile("options.txt", options[0]);
                                                        WriteToFile("options.txt", options[1]);
                                                        WriteToFile("options.txt", options[2]);
                                                        WriteToFile("options.txt", cmd[6]);
                                                        WriteToFile("options.txt", options[4]);
                                                        WriteToFile("options.txt", options[5]);
                                                        WriteToFile("options.txt", options[6]);
                                                        WriteToFile("options.txt", options[7]);
                                                        Say(chan, "Done. AutoJoin channel is now set to " + cmd[6]);
                                                    }
                                                    else
                                                    {
                                                        Say(chan, "Invalid channel name! Channel names always start with '#'");
                                                    }
                                                }
                                                else if (cmd[5].Equals("OwnerHost") || cmd[5].Equals("ownerhost"))
                                                {
                                                    WriteToFile("options.txt", options[0]);
                                                    WriteToFile("options.txt", options[1]);
                                                    WriteToFile("options.txt", options[2]);
                                                    WriteToFile("options.txt", options[3]);
                                                    WriteToFile("options.txt", "^" + cmd[6].Replace(".", "\\.").Replace("*", ".+") + "$");
                                                    WriteToFile("options.txt", options[5]);
                                                    WriteToFile("options.txt", options[6]);
                                                    WriteToFile("options.txt", options[7]);
                                                    Say(chan, "Done. My ownerhost is set to " + cmd[6] + " now.");
                                                }
                                                else if (cmd[5].Equals("CommandPrefix") || cmd[5].Equals("commandprefix"))
                                                {
                                                    WriteToFile("options.txt", options[0]);
                                                    WriteToFile("options.txt", options[1]);
                                                    WriteToFile("options.txt", options[2]);
                                                    WriteToFile("options.txt", options[3]);
                                                    WriteToFile("options.txt", options[4]);
                                                    WriteToFile("options.txt", cmd[6]);
                                                    WriteToFile("options.txt", options[6]);
                                                    WriteToFile("options.txt", options[7]);
                                                    Say(chan, "Done. Command prefix is now '" + cmd[6] + "'");
                                                }
                                                else if (cmd[5].Equals("Logging") || cmd[5].Equals("logging"))
                                                {
                                                    if (cmd[6].Equals("True") || cmd[6].Equals("False"))
                                                    {
                                                        WriteToFile("options.txt", options[0]);
                                                        WriteToFile("options.txt", options[1]);
                                                        WriteToFile("options.txt", options[2]);
                                                        WriteToFile("options.txt", options[3]);
                                                        WriteToFile("options.txt", options[4]);
                                                        WriteToFile("options.txt", options[5]);
                                                        WriteToFile("options.txt", options[6]);
                                                        WriteToFile("options.txt", cmd[6]);
                                                        Say(chan, "Done. Logging is now " + (cmd[6].Equals("True") ? "On" : "Off"));
                                                    }
                                                    else
                                                    {
                                                        Say(chan, "You must specify 'True' or 'False'!");
                                                    }
                                                }
                                                Say(chan, "This will count on next restart.");
                                            }
                                        }
                                    }
                                    string[] command = cmd[3].Split(':');
                                    Log(nick + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                                }
                            }
                            else
                            {
                                string[] command = cmd[3].Split(':');
                                Log(nick + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "restart")
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                Log(nick + " issued " + prefix + "restart");
                                writer.WriteLine("QUIT :Restarting!");
                                wentto = true;
                                goto start;
                            }
                            else
                            {
                                Log(nick + " attempted to use " + prefix + "restart");
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                            }
                        }
                        else if (cmd[3] == ":" + prefix + "hostmask" && cmd.Length > 4)
                        {
                            whoiscaller = nick;
                            whoistarget = cmd[4];
                            whoischan = cmd[2];
                            currentcmd = "Whois";
                            writer.WriteLine("WHOIS " + cmd[4]);
                            Log(nick + " issued " + prefix + "hostmask " + cmd[4]);
                        }
                    }
                    else
                    {
                        string message = string.Join(" ", cmd.Skip(3)).Substring(1);
                        if (message.StartsWith("\x01") && message.EndsWith("\x01"))
                        {
                            Log("CTCP by " + nick + ".");
                            // CTCP request
                            message = message.Trim('\x01');

                            string[] spl = message.Split(' ');
                            string ctcpCmd = spl[0];
                            string[] ctcpParams = spl.Skip(1).ToArray();

                            if (ctcpCmd == "VERSION")
                            {
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Log("Sent CTCP VERSION reply to " + nick + ".");
#endif
                                writer.WriteLine("NOTICE " + nick + " :\x01VERSION CSharpBot " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\x01");
                            }
                            else if (ctcpCmd == "PING")
                            {
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Log("Sent CTCP PING reply to " + nick + ".");
#endif
                                if (ctcpParams.Length == 0) ctcpParams = new string[] {
                                    Convert.ToString(DateTime.UtcNow.ToBinary(), 16)
                                };
                                writer.WriteLine("NOTICE " + nick + " :\x01PING " + string.Join(" ", ctcpParams) + "\x01");
                            }
                        }
                        else
                        {
                            Log("Private message by " + nick + ": " + message);
                            if (nick == "NickServ")
                                Log("NickServ identification: " + string.Join(" ", cmd.Skip(3)).Substring(1));
                            else
                              writer.WriteLine("NOTICE " + nick + " :Sorry, but you need to contact me over a channel.");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            writer.WriteLine("PRIVMSG " + CHANNEL + " : Error! Error: " + e.ToString());
            writer.WriteLine("PRIVMSG " + CHANNEL + " : Error! StackTrace: " + e.StackTrace);
            writer.WriteLine("QUIT :Exception: " + e.ToString());

            Console.ForegroundColor = ConsoleColor.Red;
            Log("The bot generated an error:");
            Log(e.ToString());
            Log("Restarting in 5 seconds...");
            Console.ResetColor();

            Thread.Sleep(5000);

            wentto = true;
            goto start; // restart
            //Environment.Exit(0); // you might also use return
        }
    }

    /// <summary>
    /// Converts a string array into a string.
    /// </summary>
    /// <param name="strArray">The string array</param>
    /// <param name="delimiter">The delimiter which should be placed between the array entries</param>
    /// <returns>A single string containing all entries delimited by a delimiter</returns>
    public static string ArrayToString(string[] strArray, string delimiter = " ")
    {
        string a = "";
        for (int b = 0; b < strArray.Length; b++)
        {
            if (b > 0)
                a += delimiter;
            a += strArray[b];
        }
        return a;
    }

    /// <summary>
    /// Checks if a hostmask fits to the owner's hostmask regex.
    /// </summary>
    /// <param name="inputmask"></param>
    /// <returns></returns>
    public static bool IsOwner(string inputmask)
    {
        return HostmaskRegex.Match(inputmask).Success;
    }
    public static void Log(string input)
    {
        if (logging == true)
        {
            string f = "Bot.log";
            if (File.Exists("Bot.log"))
            {
                List<string> lines = new List<string>();
                using (StreamReader r = new StreamReader(f))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
                using (StreamWriter w = new StreamWriter(f))
                {
                    lines.Add(input);
                    foreach (string s in lines)
                    {
                        w.WriteLine(s);
                    }
                }
            }
            else
            {
                using (StreamWriter w = new StreamWriter(f))
                {
                    w.WriteLine(input);
                }
            }
            Console.WriteLine(input);
        }
        else
        {
            Console.WriteLine(input);
        }
    }
    public static void WriteToFile(string file, string input)
    {
        if (logging == true)
        {
            if (File.Exists(file))
            {
                List<string> lines = new List<string>();
                using (StreamReader r = new StreamReader(file))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
                using (StreamWriter w = new StreamWriter(file))
                {
                    lines.Add(input);
                    foreach (string s in lines)
                    {
                        w.WriteLine(s);
                    }
                }
            }
            else
            {
                using (StreamWriter w = new StreamWriter(file))
                {
                    w.WriteLine(input);
                }
            }
            Console.WriteLine(input);
        }
        else
        {
            Console.WriteLine(input);
        }
    }
    public static void Say(string channel, string text)
    {
        if (channel.StartsWith("#"))
        {
            writer.WriteLine("PRIVMSG " + channel + " :" + text);
        }
    }
    static void SendHelp(object o)
    {
        string[] param = (string[])o;
        string nick = param[0];
        string prefix = param[1];
        writer.WriteLine("NOTICE " + nick + " :Bot commands:");
        writer.WriteLine("NOTICE " + nick + " :Everything in <> is necessary and everything in [] are optional.");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "help -- This command.");
        Thread.Sleep(1000);
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "mode <mode>-- Sets a mode the current channel.");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "topic [topic] -- Tells the current topic OR sets the channel's topic to [topic]");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "config <list|edit> [<variable> <value>] -- Tells current config.");
        Thread.Sleep(1000);
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "join <chan> -- Joins the bot to a channel");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "part <chan> [reason] -- Parts the bot from a channel");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "kick <nick> [reason] -- Kicks <nick> from the current channel for [reason], or, if [reason] is not specified, kicks user with one of the kick lines in the kicks database.");
        Thread.Sleep(1000);
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "kicklines <add|clear|read|total> <kickmessage|(do nothing)|number|(do nothing)> -- Does various actions to the kicklines database.");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "reset -- Clears the config and restarts the bot");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "restart -- Restarts the bot");
        Thread.Sleep(1000);
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "clean -- Clears the config and kills the bot");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "die [quitmessage] -- Kills the bot, with optional [quitmessage]");
        writer.WriteLine("NOTICE " + nick + " :" + prefix + "time [<+|-> <number>] -- Tells the time in GMT/UTC, with the offset you specify.");
    }
}