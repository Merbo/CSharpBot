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
    // RegEx'es
    static Regex HostmaskRegex;

    // Streams
    static NetworkStream stream;
    static TcpClient irc;
    static StreamWriter writer;
    static StreamReader reader;

    // Configuration
    public static string CurrentLine; // Current IRC line, sometimes used as temporary string memory
    public static string Prefix = ".";
    public static string OwnerHostPattern = "*!*@localhost";
    public static string Channel = "#csharpbot";
    public static string Nickname = "CSharpBot";
    public static string Server = "irc.merbosmagic.co.cc";
    public static string Username = "CSharpBot";
    public static string Realname = "MerbosMagic C# Bot";
    public static int Port = 6667;

    #region IRC formatting implementation
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

    #region Logging implementation by Icedream
    public static List<TextWriter> LoggingOutputs;
    public static void InitializeOutputs(IEnumerable<string> types)
    {
        LoggingOutputs = new List<TextWriter>();
        foreach (string t in types)
        {
            switch (t.Substring(0, 1))
            {
                case "f":
                    string filename = t.Substring(2);
                    LoggingOutputs.Add(new StreamWriter(filename));
                    break;
                case "s":
                    LoggingOutputs.Add(Console.Out);
                    break;
            }
        }
    }
    public static void DeinitializeOutputs()
    {
        foreach (TextWriter tw in LoggingOutputs)
            tw.Close();
    }
    public static void WriteLine(object value = null)
    {
        if (LoggingOutputs != null)
        {
            foreach (TextWriter tw in LoggingOutputs)
            {
                tw.WriteLine(/*"[{0}] {1}", DateTime.Now, */value);
                tw.Flush();
            }
        }
        else Console.WriteLine(value);
    }
    public static void Write(object value)
    {
        if (LoggingOutputs != null)
        {
        foreach (TextWriter tw in LoggingOutputs)
            tw.Write(value);
        }
        else Console.Write(value);
    }
    static List<string> LogType = new List<string>();
    static List<StreamWriter> LogOutputs = new List<StreamWriter>();
#endregion

    public static bool FirstUseSetup()
    {

        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        WriteLine("=== First Use Configuration ===");
        WriteLine("");
        Console.ResetColor();

        // The SERVER
        Console.ForegroundColor = ConsoleColor.Cyan;
        Write("Server [Default: " + Server + "]: ");
        Console.ForegroundColor = ConsoleColor.White;
        CurrentLine = Console.ReadLine();
        if (CurrentLine != "") Server = CurrentLine;

        // The PORT
        Console.ForegroundColor = ConsoleColor.Cyan;
        bool portOK = false;
        while (!portOK)
        {
            Write("Port [Default: " + Port.ToString() + "]: ");

            // Errors?
            Console.ForegroundColor = ConsoleColor.White;
            int num;
            bool validNumber = int.TryParse(CurrentLine = Console.ReadLine(), out num);
            if (CurrentLine == "")
            {
                num = Port;
                validNumber = true;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            if (!validNumber)
                WriteLine("Sorry, but this is an invalid port number!");
            else if (Port > 0xffff)
                WriteLine("Sorry, but this is a too big number.");
            else if (Port < 0)
                WriteLine("Sorry, but this is a too small number.");
            else
            {
                Port = num;
                portOK = true;
            }
        }

        // The NICKname
        Console.ForegroundColor = ConsoleColor.Cyan;
        Write("Nickname [Default: " + Nickname + "]: ");
        Console.ForegroundColor = ConsoleColor.White;
        CurrentLine = Console.ReadLine();
        if (CurrentLine != "") Nickname = CurrentLine;

        // The Username
        Console.ForegroundColor = ConsoleColor.Cyan;
        Write("Username [Default: " + Username + "]: ");
        Console.ForegroundColor = ConsoleColor.White;
        CurrentLine = Console.ReadLine();
        if (CurrentLine != "") Username = CurrentLine;

        // The Realname
        Console.ForegroundColor = ConsoleColor.Cyan;
        Write("Realname [Default: " + Realname + "]: ");
        Console.ForegroundColor = ConsoleColor.White;
        CurrentLine = Console.ReadLine();
        if (CurrentLine != "") Realname = CurrentLine;

        // The CHANNEL
        while (!Channel.StartsWith("#"))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Write("Channel [Default: " + Channel + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            CurrentLine = Console.ReadLine();
            if (CurrentLine != "") Channel = CurrentLine;
            Console.ForegroundColor = ConsoleColor.Red;
            if (!Channel.StartsWith("#"))
                WriteLine("Sorry, but channel names always begin with #!");
        }

        // The ownerhost
        while (HostmaskRegex == null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Write("Hostmask of owner (Nickname!Username@Host, can be a regex globmask (wildcards like * and ?)) [Default: " + OwnerHostPattern + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            CurrentLine = Console.ReadLine();
            if (CurrentLine != "") OwnerHostPattern = CurrentLine;
            try
            {
                HostmaskRegex = new Regex(OwnerHostPattern = "^" + OwnerHostPattern.Replace(".", "\\.").Replace("*", ".+") + "$");
#if DEBUG
                         Console.ForegroundColor = ConsoleColor.Yellow;
                         WriteLine("(debug) Parsed Regex: " + ownerhost);
                         Console.ResetColor();
#endif
            }
            catch (Exception n)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("Something went wrong: " + n.Message);
                WriteLine("Exception: " + n.ToString());
                WriteLine("StackTrace: " + n.StackTrace);
            }
        }

        // The prefix
        while (Prefix.Length < 1)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Write("Command Prefix (e.g. in '!kick' it is '!') [Default: " + Prefix + "]: ");
            Console.ForegroundColor = ConsoleColor.White;
            CurrentLine = Console.ReadLine();
            if (CurrentLine != "") Prefix = CurrentLine;
            Console.ForegroundColor = ConsoleColor.Red;
            if (Prefix.Length < 1)
                WriteLine("You must set a prefix!");
        }

        // Our logging options
        LogType = new List<string>();
        bool logtypesel = false;
        Console.ForegroundColor = ConsoleColor.Cyan;
        WriteLine("Please select logging types. To finish press [Enter]. You may choose between:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteLine(" [s] Screen logging");
        WriteLine(" [f] File logging");
        //WriteLine(" [l] System logging"); // to be implemented
        while (!logtypesel)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Write("Please type:");
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleKey n = Console.ReadKey().Key;
            if (n == ConsoleKey.F)
            {
                bool yes = true;
                foreach (string y in LogType)
                    if (y.StartsWith("f"))
                    {
                        yes = false;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        WriteLine("Already activated file logging.");
                        break;
                    }
                if (yes)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    WriteLine();
                    Write("Log file name? ");
                    Console.ForegroundColor = ConsoleColor.White;
                    LogType.Add("f:" + Console.ReadLine()); // example: f:screen.log => Type: [F]ile, Filename: screen.log
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLine("Enabled file logging");
                }
            }
            else if (n == ConsoleKey.S)
            {
                if (LogType.Contains("s"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    WriteLine();
                    WriteLine("Already activated screen logging.");
                }
                else
                {
                    LogType.Add("s");
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLine();
                    WriteLine("Enabled screen logging");
                }
            }
            else if (n == ConsoleKey.Enter)
                logtypesel = true;
        }

        // Finishing configuration...
        Console.ResetColor();
        WriteLine();
        Username = "USER " + Nickname + " 8 * :" + Realname;
        string[] options = { Server, Port.ToString(), Nickname, Channel, OwnerHostPattern, Prefix, Username, string.Join(",", LogType) };
        try
        {
            File.WriteAllLines("options.txt", options);
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("Configuration has been saved successfully. The bot will now start!");
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("Configuration has NOT been saved. Please check if the directory is writeable for the bot.");
            WriteLine("Enter something to exit.");
            Console.ReadKey();
            return false;
        }
        return true;
    }

    public static void ShowVersion()
    {
        WriteLine("CSharpBot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        WriteLine("\t(c) Merbo August 3, 2011-Present");
        WriteLine("\t(c) Icedream August 5, 2011-Present");
        WriteLine();
    }

    static void Main(string[] args)
    {
        bool wentto = false;

        start: // This is the point at which the bot restarts on errors

        Program.LoggingOutputs = null;
        Program.LogOutputs = null;
        Program.LogType = null;

        if (wentto == true)
        {
            WriteLine("");
            HostmaskRegex = null;
            wentto = false;
        }
        Console.ResetColor();

        ShowVersion();

        // First setup
        if (!File.Exists("Options.txt"))
            if (!FirstUseSetup())
                // fail
                return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        WriteLine("Loading configuration...");
        try
        {
            string[] options = File.ReadAllLines("options.txt");
            Server = options[0];
            Port = int.Parse(options[1]);
            Nickname = options[2];
            Channel = options[3];
            OwnerHostPattern = options[4];
            HostmaskRegex = new Regex(options[4]);
            Prefix = options[5];
            Username = options[6];
            LogType = new List<string>();
            LogType.AddRange(options[7].Split(','));
            InitializeOutputs(LogType);
        }
        catch (
            Exception
#if DEBUG
            e
#endif
        )
        {
            Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
            WriteLine(e);
#endif
            WriteLine("Configuration has NOT been loaded. Please check if the configuration is valid and try again.");
            WriteLine("Enter something to exit.");
            Console.ReadKey();
            return;
        }

        try
        {
            WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Write("Connecting to " + Server + "... ");
            
            irc = new TcpClient(Server, Port);
            if (!irc.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("failed! Bot is restarting in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                wentto = true;
                goto start;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("ok!");
            Console.ForegroundColor = ConsoleColor.White;
            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            Write("Logging in... ");
            writer.WriteLine(Username);
            writer.WriteLine("NICK " + Nickname);
            string currentcmd = null;
            string whoiscaller = null;
            string whoistarget = null;
            string whoischan = null; 
            while ((CurrentLine = reader.ReadLine()) != null)
            {
                string[] cmd = CurrentLine.Split(' ');
                #if DEBUG
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!cmd[0].Equals("PING"))
                    WriteLine("<= " + inputline);
                    Console.ResetColor();
                #endif
                if (cmd[0].Equals("PING")) {
                    #if DEBUG
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        WriteLine("Ping? Pong!");
                        Console.ResetColor();
                    #endif
                    writer.WriteLine("PONG " + cmd[1]);
                }

                if (cmd[1].Equals("376"))
                {
                    #if DEBUG
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        WriteLine("[motd received] ");
                        Console.ResetColor();
                    #endif
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLine("ok!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Write("Applying optimal flags... ");
                    writer.WriteLine("MODE " + Nickname + " +B"); // We are a bot
                    writer.WriteLine("MODE " + Nickname + " +w"); // We want to get wallops, if any
                    writer.WriteLine("MODE " + Nickname + " -i"); // We don't want to be invisible
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLine("ok!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Write("Joining " + Channel + "... ");
                    writer.WriteLine("JOIN " + Channel);
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLine("ok!");
                    Console.ForegroundColor = ConsoleColor.White;
                    WriteLine("Now ready for use.");
                }
                else if (cmd[1].Equals("311"))
                {
                    if (currentcmd.Equals("Whois") && cmd.Length > 6)
                    {
                        WriteLine("Reading WHOIS to get hostmask of " + whoistarget + " for " + whoiscaller + "...");
                        writer.WriteLine("PRIVMSG " + whoischan + " :" + whoiscaller + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                        WriteLine("Found the hostmask that " + whoiscaller + " called for, of " + whoistarget + "'s hostmask, which is: " + cmd[5]);
                    }
                }
                else if (cmd[1].Equals("KICK") && cmd[3] == Nickname)
                {
                    WriteLine("Rejoining " + cmd[2]);
                    writer.WriteLine("JOIN " + cmd[2]);
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

                    // Execute commands
                    if (cmd[3] == ":" + Prefix + "test")
                    {
                        WriteLine(nick + " issued " + Prefix + "test");
                        writer.WriteLine("PRIVMSG " + chan + " :I think your test works ;-)");
                    }
                    else if (cmd[3] == ":" + Prefix + "amiowner")
                    {
                        WriteLine(nick + " issued " + Prefix + "amiowner");
                        writer.WriteLine("PRIVMSG " + chan + " :The answer is: " + (IsOwner(prenick1[1]) ? "Yes!" : "No!"));
                    }
                    else if (cmd[3] == ":" + Prefix + "time")
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

                        if(cmd.Length > 4)
                            WriteLine(nick + " issued " + Prefix + "time " + cmd[4]);
                        else
                            WriteLine(nick + " issued " + Prefix + "time");
                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": It's " + DateTime.UtcNow.AddHours(add).ToString() + "(UTC" + adds + ")");
                    }
                    else if (cmd[3] == ":" + Prefix + "mynick")
                    {
                        WriteLine(nick + " issued " + Prefix + "mynick");
                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your nick is " + nick);
                    }
                    else if (cmd[3] == ":" + Prefix + "myident")
                    {
                        WriteLine(nick + " issued " + Prefix + "myident");
                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your ident is " + ident);
                    }
                    else if (cmd[3] == ":" + Prefix + "myhost")
                    {
                        WriteLine(nick + " issued " + Prefix + "myhost");
                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your host is " + host);
                    }
                    else if (cmd[3] == ":" + Prefix + "myfullmask")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Your full mask is " + cmd[0]);
                    }
                    else if (cmd[3] == ":" + Prefix + "die")
                    {
                        if (IsOwner(prenick1[1]))
                        {
                            WriteLine(nick + " issued " + Prefix + "die");
                            writer.WriteLine("QUIT :I shot myself because " + nick + " told me to.");
                            DeinitializeOutputs();
                        }
                        else
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "die");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "clean")
                    {
                        if (IsOwner(prenick1[1]))
                        {
                            FileInfo fi = new FileInfo("options.txt");
                            fi.Delete();
                            WriteLine(nick + " issued " + Prefix + "clean");
                            writer.WriteLine("QUIT :Cleaned!");
                        }
                        else
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "clean");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                        }
                    }
                    
                    else if (cmd[3] == ":" + Prefix + "topic")
                    {
                        if (cmd.Length > 4)
                        {
                            cmd[4] = cmd[4] == "reset" ? "" : cmd[4]; // !topic reset = set topic to ""

                            // Set topic if is owner
                            if (IsOwner(prenick1[1]))
                            {
                                WriteLine(nick + " issued " + Prefix + "topic (set topic)");
                                writer.WriteLine("TOPIC " + chan + " :" + ArrayToString(cmd.Skip(4).ToArray(), " "));
                                writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Topic has been set.");
                            }
                            else
                            {
                                WriteLine(nick + " attempted to use " + Prefix + "topic (set topic).");
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
                                    WriteLine(topic);
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
                            WriteLine(nick + " issued " + Prefix + "topic (read topic).");
                        }
                    }
                    else if (cmd[3] == ":GTFO")
                    {
                        if (cmd.Length > 4)
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                WriteLine(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + cmd[4]);
                                writer.WriteLine("KICK " + chan + " " + cmd[4] + " :GTFO!");
                            }
                            else
                            {
                                WriteLine(nick + " told " + cmd[4] + " to GTFO of " + chan + ", so I kicked " + nick + " for being mean.");
                                writer.WriteLine("KICK " + chan + " " + nick + " :NO U");
                            }
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "kicklines")
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
                                WriteLine(nick + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                            }
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                            string[] command = cmd[3].Split(':');
                            WriteLine(nick + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(5).ToArray()));
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "kick")
                    {
                        if (cmd.Length > 4)
                        {
                            if (IsOwner(prenick1[1]))
                            {
                                if (File.Exists("Kicks.txt"))
                                {
                                    string[] lines = File.ReadAllLines("Kicks.txt");
                                    Random rand = new Random();
                                    writer.WriteLine("KICK " + chan + " " + cmd[4] + " :" + lines[rand.Next(lines.Length)]);
                                    WriteLine(nick + " issued " + Prefix + "kick " + cmd[4]);
                                }
                                else
                                {
                                    writer.WriteLine("KICK " + chan + " " + cmd[4] + " :Goodbye! You just got kicked by " + nick + ".");
                                    WriteLine(nick + " issued " + Prefix + "kick " + cmd[4]);
                                }
                                //  writer.WriteLine("KICK " + chan + " " + cmd[4] + " Gotcha! You just got ass-kicked by " + nick + "."); // might also be an idea ;D
                            }
                            else
                            {
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                                WriteLine(nick + " attempted to use " + Prefix + "kick " + cmd[4]);
                            }
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "join")
                    {
                        if (IsOwner(prenick1[1]) && cmd.Length > 4)
                        {
                            WriteLine(nick + " issued " + Prefix + "join " + cmd[4]);
                            writer.WriteLine("JOIN " + cmd[4]);
                        }
                        else if (!IsOwner(prenick1[1]))
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "join " + cmd[4]);
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "mode")
                    {
                        if (IsOwner(prenick1[1]))
                        {
                            if (cmd.Length > 5)
                            {
                                WriteLine(nick + " issued " + Prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                writer.WriteLine("MODE " + chan + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                            }
                            else if (cmd.Length > 4)
                            {
                                WriteLine(nick + " issued " + Prefix + "mode " + cmd[4] + " on " + chan);
                                writer.WriteLine("MODE " + chan + " " + cmd[4]);
                            }
                        }
                        else if (!IsOwner(prenick1[1]))
                        {
                            if (cmd.Length > 5)
                            {
                                WriteLine(nick + " attempted to use " + Prefix + "mode " + string.Join(" ", cmd.Skip(4).ToArray()) + " on " + chan);
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                            }
                            else if (cmd.Length > 4)
                            {
                                WriteLine(nick + " attempted to use " + Prefix + "mode " + cmd[4] + " on " + chan);
                                writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                            }
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "part")
                    {
                        if (IsOwner(prenick1[1]) && cmd.Length > 4)
                        {
                            WriteLine(nick + " issued " + Prefix + "part " + cmd[4]);
                            writer.WriteLine("PART " + cmd[4]);
                        }
                        else if (!IsOwner(prenick1[1]))
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "join " + cmd[4]);
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "reset")
                    {
                        if (IsOwner(prenick1[1]))
                        {
                            FileInfo fi = new FileInfo("options.txt");
                            fi.Delete();
                            WriteLine(nick + " issued " + Prefix + "reset");
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Configuration reset. The bot will now restart.");
                            writer.WriteLine("QUIT :Resetting!");
                            wentto = true;
                            goto start;
                        }
                        else
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "reset");
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "config")
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
                                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Owner host: " + options[4].Replace(".+", "*").Replace("^", "").Replace("$", ""));
                                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": Command Prefix: " + options[5]);
                                    }
                                    else
                                    {
                                        writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": I'd love to tell you, but there isn't a configuration file :|");
                                    }
                                }
                                string[] command = cmd[3].Split(':');
                                WriteLine(nick + " issued " + command[1] + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                            }
                        }
                        else
                        {
                            string[] command = cmd[3].Split(':');
                            WriteLine(nick + " attempted to use " + command[1] + " " + string.Join(" ", cmd.Skip(4).ToArray()));
                            writer.WriteLine("PRIVMSG " + chan + " :" + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "restart")
                    {
                        if (IsOwner(prenick1[1]))
                        {
                            WriteLine(nick + " issued " + Prefix + "restart");
                            writer.WriteLine("QUIT :Restarting!");
                            wentto = true;
                            goto start;
                        }
                        else
                        {
                            WriteLine(nick + " attempted to use " + Prefix + "restart");
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + Prefix + "hostmask" && cmd.Length > 4)
                    {
                        whoiscaller = nick;
                        whoistarget = cmd[4];
                        whoischan = cmd[2];
                        currentcmd = "Whois";
                        writer.WriteLine("WHOIS " + cmd[4]);
                        WriteLine(nick + " issued " + Prefix + "hostmask " + cmd[4]);
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (writer != null)
            {
                writer.WriteLine("PRIVMSG " + Channel + " : Error! Error: " + e.ToString());
                writer.WriteLine("PRIVMSG " + Channel + " : Error! StackTrace: " + e.StackTrace);
                writer.WriteLine("QUIT :Exception: " + e.ToString());
                writer.Close();
            }

            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("The bot generated an error:");
            WriteLine(e);
            WriteLine("Restarting in 5 seconds...");
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
}