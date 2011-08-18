using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using Client;

namespace Client
{
    public class Client
    {
        public static string server, nick, pass;
        public static int port;

        // Setting up socket, reader & writer   
        public static TcpClient socket;
        public static StreamReader reader;
        public static StreamWriter writer;

        //Tells us whether we are debugging - only enabled via arguement --debug
        public static bool Debug;

        //Creating aliases for our functions :)
        static string read()
        {
            return Functions.read();
        }
        static string read(StreamReader r)
        {
            return Functions.read(r);
        }
        static void write(string data)
        {
            Functions.write(data);
        }
        static void write(string data, StreamWriter r)
        {
            Functions.write(data, r);
        }

        static void Log(string data, int level = 6)
        {
            /*
             * <> Means necessary parameter
             * [] Means optional parameter
             * 
             * Syntax: Log(<string>, [int])
             * 
             * Action: Writes <string> to the console, also can use presets based off of simple tasks, via use of [int]
             * 
             * By default, this will simply do a white console.WriteLine of <string>.
             * However, [int] can be:
             * 0: Error
             * 1: Warning
             * 2: Notice
             * 3: Debug*
             * 4: Log*
             * 5: Setup**
             * 6: Plain old Console.WriteLine
             * (*: Only runs if 'Debug' evaluates to true)
             * (**: Only meant for use at the beginning setup configuration.) 
            */
            Functions.Log(data, level);
        }

        static void Main(string[] args)
        {
            //process args.
            foreach (string arg in args)
            {
                string[] parameters = arg.Split('=');
                string name = parameters[0].ToLower();
                string value = string.Join("=", parameters.Skip(1).ToArray());
                switch (name)
                {
                    case "-d":
                    case "--debug":
                        Debug = true;
                        continue;
                }
            }


            tryserver:
            Log("Server: ", 5);
            server = Console.ReadLine();
            if (server == "")
                goto tryserver;
            tryport:
            Log("Port: ", 5);
            string tmp = Console.ReadLine();
            if (int.TryParse(tmp, out port))
            {
                if (port > 0 && port < 65535)
                {
                }
                else
                {
                    Log("Invalid port.", 0);
                    goto tryport;
                }
            }
            else
            {
                Log("Invalid Port.", 0);
                goto tryport;
            }
            trynick:
            Log("Nick: ", 5);
            nick = Console.ReadLine();
            if (nick == "")
                goto trynick;
            Log("Pass (if given): ", 5);
            pass = Console.ReadLine();
            try
            {
                socket = new TcpClient(server, port);
                socket.ReceiveBufferSize = 1024;
                Console.WriteLine("Connected :D");
                NetworkStream stream = socket.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                Log("Logging in...");
                write("NICK " + nick);
                if (pass != "")
                {
                    Log("Sending password...");
                    write("PASS " + pass);
                }
                string input = Console.ReadLine().ToLower();
                while (!input.Equals("/quit"))
                {
                    string data = read(reader);
                    if (data.StartsWith("/"))
                    {
                        string[] split = data.Split(' ');
                        string[] cmd = split[0].Split('/');
                        string aftercommand = string.Join(" ", split.Skip(1));
                        switch (cmd[1])
                        {
                            case "say":
                                write("PRIVMSG " + input);
                                break;
                            case "nick":
                                write("NICK " + aftercommand);
                                break;
                            case "kill":
                                write("KILL " + aftercommand);
                                break;
                            case "quit":
                                write("QUIT " + aftercommand);
                                break;
                            default:
                                Log("You cant just create your own commands!", 1);
                                break;
                        }
                    }
                    else
                        write(input);
                }
                reader.Close();
                writer.Close();
                stream.Close();
            }
            catch
            {
                Log("Failed to connect", 0);
            }
            Log("Terminating...", 3);
            Console.ReadKey();
        }
    }
}