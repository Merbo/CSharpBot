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
        string inputline;
        StreamReader reader;
        string prefix;
        string ownerhost;
        string CHANNEL;
        string NICK;
        int PORT;
        string SERVER;
        string USER;
        if (!System.IO.File.Exists("Options.txt"))
        {
            Console.Write("Server: ");
            SERVER = Console.ReadLine();
            Console.Write("Port: ");
            PORT = int.Parse(Console.ReadLine());
            Console.Write("Nick: ");
            NICK = Console.ReadLine();
            Console.Write("Channel: ");
            CHANNEL = Console.ReadLine();
            Console.Write("Host (after the @) of owner: ");
            ownerhost = Console.ReadLine();
            Console.Write("Command Prefix (e.g. in '!kick' it is '!'): ");
            prefix = Console.ReadLine();
            USER = "USER " + NICK + " 8 * :Merbo's C# Bot";
            string[] options = { SERVER, PORT.ToString(), NICK, CHANNEL, ownerhost, prefix, USER };
            System.IO.File.WriteAllLines("options.txt", options);
        }
        else
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
        try
        {
            irc = new TcpClient(SERVER, PORT);
            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            writer.WriteLine(USER);
            writer.WriteLine("NICK " + NICK);
            string currentcmd = null;
            string whoiscaller = null;
            string whoistarget = null;
            string whoischan = null; 
            while ((inputline = reader.ReadLine()) != null)
            {
                Console.WriteLine(inputline);
                string[] cmd = inputline.Split(' ');
                if (cmd[0].Equals("PING")) {
                    writer.WriteLine("PONG " + cmd[1]);
                }
                else if (cmd[1].Equals("376"))
                {
                    writer.WriteLine("JOIN " + CHANNEL);
                }
                else if (cmd[1].Equals("311") && currentcmd.Equals("Whois") && cmd.Length > 6)
                {
                    writer.WriteLine("PRIVMSG " + whoischan + " :" + whoiscaller + ": " + whoistarget + "'s hostmask is " + cmd[5]);
                }
                else if (cmd[1].Equals("KICK") && cmd[3] == NICK)
                {
                    writer.WriteLine("JOIN " + cmd[2]);
                }
                else if (cmd[1].Equals("PRIVMSG"))
                {
                    string[] prenick1 = cmd[0].Split(':');
                    string[] prenick = prenick1[1].Split('!');
                    string nick = prenick[0];
                    string[] preident = prenick[1].Split('@');
                    string ident = preident[0];
                    string host = preident[1];
                    string chan = cmd[2];
                    if (cmd[3] == ":" + prefix + "test")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " :Test! :D");
                    }
                    else if (cmd[3] == ":" + prefix + "mynick")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": " + nick);
                    }
                    else if (cmd[3] == ":" + prefix + "myident")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": " + ident);
                    }
                    else if (cmd[3] == ":" + prefix + "myhost")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": " + host);
                    }
                    else if (cmd[3] == ":" + prefix + "myfullmask")
                    {
                        writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": " + cmd[0]);
                    }
                    else if (cmd[3] == ":" + prefix + "die")
                    {
                        if (host == ownerhost)
                        {
                            writer.WriteLine("QUIT :" + nick + " told me to. :p");
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "mode")
                    {
                        if (host == ownerhost)
                        {
                            writer.WriteLine("MODE " + chan + " " + cmd[4]);
                        }
                        else
                        {
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": You are not my owner!");
                        }
                    }
                    else if (cmd[3] == ":" + prefix + "kick")
                    {
                        if (host == ownerhost)
                        {
                                writer.WriteLine("KICK " + chan + " " + cmd[4] + " Goodbye. Kicked by " + nick + ".");
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
                            writer.WriteLine("PRIVMSG " + chan + " : " + nick + ": Done.");
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
            Thread.Sleep(5000);
            Environment.Exit(0);
        }
    }
}