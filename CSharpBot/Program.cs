using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;

class Program
{
    public static StreamWriter writer;               
    static void Main(string[] args)
    {
        NetworkStream stream;
        TcpClient irc;
        StreamReader reader;

        string inputline;
        string prefix = "";
        string ownerhost;
        string CHANNEL = "";
        string NICK;
        string SERVER;
        string USER;
        int PORT = -1; // set to -1 for later validation of input

        // Head-lines
        Console.WriteLine("CSharpBot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        Console.WriteLine("\t(c) Merbo August 3, 2011-Present");
        Console.WriteLine("\t(c) Icedream August 5, 2011-Present");
        Console.WriteLine();

        start: // This is the point at which the bot restarts on errors

        // First setup
        if (!System.IO.File.Exists("Options.txt"))
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Host (after the @) of owner: ");
            Console.ForegroundColor = ConsoleColor.White;
            ownerhost = Console.ReadLine();

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

            // Finishing configuration...
            Console.ResetColor();
            Console.WriteLine();
            USER = "USER " + NICK + " 8 * :Merbo's and Icedream's C# Bot";
            string[] options = { SERVER, PORT.ToString(), NICK, CHANNEL, ownerhost, prefix, USER };
            try
            {
                System.IO.File.WriteAllLines("options.txt", options);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration has been saved successfully. You might now start using the bot!");
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
            Console.WriteLine("Loading configuration...");
            try
            {
                string[] options = System.IO.File.ReadAllLines("options.txt");
                SERVER = options[0];
                PORT = int.Parse(options[1]);
                NICK = options[2];
                CHANNEL = options[3];
                ownerhost = options[4];
                prefix = options[5];
                USER = options[6];
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
            Console.WriteLine("Connecting to " + SERVER);
            
            irc = new TcpClient(SERVER, PORT);
            if (!irc.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Connection failed. Bot is restarting in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                goto start;
            }

            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            Console.WriteLine("Logging in...");
            writer.WriteLine(USER);
            writer.WriteLine("NICK " + NICK);
            string currentcmd = null;
            string whoiscaller = null;
            string whoistarget = null;
            string whoischan = null; 
            while ((inputline = reader.ReadLine()) != null)
            {
                Console.WriteLine("<= " + inputline);
                string[] cmd = inputline.Split(' ');
                if (cmd[0].Equals("PING")) {
                    //Console.WriteLine("=> Sent PONG back"); // it might go on the console's nerves
                    writer.WriteLine("PONG " + cmd[1]);
                }
                else if (cmd[1].Equals("376"))
                {
                    Console.WriteLine("=> Joining " + CHANNEL + "...");
                    writer.WriteLine("JOIN " + CHANNEL);
                }
                else if (cmd[1].Equals("311") && currentcmd.Equals("Whois") && cmd.Length > 6)
                {
                    Console.WriteLine("=> Responding on WHOIS...");
                    writer.WriteLine("PRIVMSG " + whoischan + " :" + whoiscaller + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                }
                else if (cmd[1].Equals("KICK") && cmd[3] == NICK)
                {
                    Console.WriteLine("=> Respawning in " + cmd[2]);
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
                    if (cmd[3] == ":" + prefix + "test")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " :I think your test works ;-)");
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
                        else 
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": It's " + DateTime.UtcNow.AddHours(add).ToString() + "(UTC" + adds + ")");
                    }
                    else if (cmd[3] == ":" + prefix + "mynick")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Your nick is " + nick);
                    }
                    else if (cmd[3] == ":" + prefix + "myident")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Your ident is " + ident);
                    }
                    else if (cmd[3] == ":" + prefix + "myhost")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Your host is " + host);
                    }
                    else if (cmd[3] == ":" + prefix + "myfullmask")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Your full mask is " + cmd[0]);
                    }
                    else if (cmd[3] == ":" + prefix + "die")
                    {
                        if (host == ownerhost)
                        {
                            writer.WriteLine("QUIT :" + nick + " just fired me >:(");
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":GTFO")
                    {
                        if (host == ownerhost)
                        {
                            writer.WriteLine("KICK " + chan + " " + cmd[4] + " :GTFO!");
                        }
                        else
                        {
                            writer.WriteLine("KICK " + chan + " " + nick + " :NO U");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "kick")
                    {
                        if (host == ownerhost)
                        {
                            writer.WriteLine("KICK " + chan + " " + cmd[4] + " Goodbye! You just got kicked by " + nick + ".");
                            //  writer.WriteLine("KICK " + chan + " " + cmd[4] + " Gotcha! You just got ass-kicked by " + nick + "."); // might also be an idea ;D
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "join")
                    {
                        if (host == ownerhost && cmd[4] != null)
                        {
                            writer.WriteLine("JOIN " + cmd[4]);
                        }
                        else if (host != ownerhost)
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "part")
                    {
                        if (cmd.Length > 4)
                        {
                            writer.WriteLine("PART " + cmd[4]);
                        }
                        else if (host != ownerhost)
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "reset")
                    {
                        if (host == ownerhost)
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo("options.txt");
                            fi.Delete();
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Configuration resetted. You might start at the beginning.");
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "hostmask" && cmd[4] != null)
                    {
                        whoiscaller = nick;
                        whoistarget = cmd[4];
                        whoischan = cmd[2];
                        currentcmd = "Whois";
                        writer.WriteLine("WHOIS " + cmd[4]);
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
            Console.WriteLine("The bot generated an error:");
            Console.WriteLine(e);
            Console.WriteLine("Restarting in 5 seconds...");
            Console.ResetColor();

            Thread.Sleep(5000);
            goto start; // restart
            //Environment.Exit(0); // you might also use return
        }
    }
}