using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SimpleLiveClient
{
    class Program
    {
        public static TcpClient client = new TcpClient();
        public static NetworkStream clientStream;
        public static ASCIIEncoding encoder = new ASCIIEncoding();
        public static string input;
        public static string nick;
        public static string server;
        public static string pass;


        static void Main(string[] args)
        {
            try
            {
                bool hasauth = false; // I don't know if we still need this
                bool haspass = false;
                retryserver:
                Console.Write("Server: ");
                server = Console.ReadLine();
                if (server == "")
                    goto retryserver;
                retrynick:
                Console.Write("Nick: ");
                nick = Console.ReadLine();
                if (nick == "")
                    goto retrynick;
                Console.Write("Password(If any): ");
                pass = Console.ReadLine();
                if (pass != "")
                    haspass = true;
                Console.WriteLine();
                Console.WriteLine("Connecting to " + server + "...");
                client.Connect(server, 3000);
                Console.WriteLine("Connection was " + (client.Connected ? "" : "not ") + "successful!");
                // client.Connect(serverEndPoint);
                Console.WriteLine();
                clientStream = client.GetStream();
                Console.WriteLine("Setting nickname...");
                input = "/nick";

                int bytesRead;
                byte[] message = new byte[2048];
                while (input.ToLower() != "/quit")
                {
                    bytesRead = 0;
                    byte[] buffer = encoder.GetBytes(input);
                    string bfr = encoder.GetString(buffer);
                    
                    if (bfr.StartsWith("/")) // user command
                    {
                        string[] spl = bfr.Substring(1).Split(' ');
                        string command = spl[0].ToUpper();
                        string[] param = spl.Skip(1).ToArray();

                        
                        switch (command)
                        {
                            case "CMD":
                                command = "*" + param[0];
                                param = param.Skip(1).ToArray();
                                break;
                            case "AUTH":
                            case "PASS":
                                param = new string[] { pass };
                                command = "PASS";
                                break;
                            case "NICK":
                                param = new string[] { nick };
                                break;
                            //default: // This may be a message?
                            //    param = (command + " " + string.Join(" ", param)).Split(' ');
                            //    command = "MSG";
                            //    break;
                        }

                        bfr = command + " " + string.Join(" ", param);
                        //Console.WriteLine("client>" + bfr);
                    }
                    SendBytes(bfr);
                    clientStream.Flush();
                    
                    try
                    {
                        //blocks until a client sends a message
                        if (clientStream.CanRead)
                            bytesRead = clientStream.Read(message, 0, 2048);
                    }
                    catch (Exception e)
                    {
                        //a socket error has occured
                        Console.WriteLine(e.ToString());
                        break;
                    }
                    string decodedMessage = encoder.GetString(message, 0, bytesRead);
                    string[] msg = decodedMessage.Split(' ');
                    switch (msg[0])
                    {
                        case "000":
                            return;
                        case "001":
                            Console.WriteLine("(001) Message Sent!");
                            break;
                        case "002":
                            Console.WriteLine("(002) " + msg[1] + " joined the party line.");
                            break;
                        case "003":
                            Console.WriteLine("(003) You have been authenticated!");
                            hasauth = true;
                            break;
                        case "004":
                            Console.WriteLine("(004) <" + msg[1] + "> " + string.Join(" ", msg.Skip(2)));
                            break;
                        case "005":
                            Console.WriteLine("(005) Nickname successfully set.");
                            break;
                        case "006":
                            Console.WriteLine("(006) Missing arguments.");
                            break;
                        case "007":
                            Console.WriteLine("(007) Too much arguments.");
                            break;
                        case "008":
                            Console.WriteLine("(008) Invalid arguments.");
                            break;
                        case "009":
                            Console.WriteLine("(009) You need to set your nickname first."); // THIS SHOULD NEVER APPEAR. Would be silly. If you don't know, think deeply.
                            break;
                        case "010":
                            Console.WriteLine("(010) Invalid password.");
                            break;
                        case "011":
                            Console.WriteLine("(011) Huh? (Sending messages works with \"/msg your text\")");
                            break;
                        case "012":
                            Console.WriteLine("(012) Permission denied. You're not allowed to do this.");
                            break;
                        case "020":
                            Console.WriteLine("(020) Livescript executed.");
                            break;
                        case "021":
                            Console.WriteLine("(021) Livescript failed.");
                            break;
                        default:
                            Console.WriteLine("(???) Unknown response from server:");
                            Console.WriteLine(msg[0]);
                            break;
                    }

                retrycmd:
                    Console.Write("client>");
                    input = Console.ReadLine();
                    if (input.Trim() == "") goto retrycmd;
                }
                client.Close();
            }
            catch (Exception)
            {
                //Environment.Exit(0);
                return;
            }
        }
        public static void SendBytes(string input)
        {
            byte[] bytes = encoder.GetBytes(
                //nick + " " +
                input
                );
            clientStream.Write(bytes, 0, bytes.Length);
        }
    }
}
