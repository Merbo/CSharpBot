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
                bool hasauth = false;
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
                client.Connect(server, 3000);
                // client.Connect(serverEndPoint);
                Console.WriteLine();
                clientStream = client.GetStream();
                retrycmd:
                input = Console.ReadLine();
                int bytesRead;
                byte[] message = new byte[4096];
                while (input.ToLower() != "/quit")
                {
                    bytesRead = 0;
                    byte[] buffer = encoder.GetBytes(input);
                    string bfr = encoder.GetString(buffer);
                    if (bfr.StartsWith("/"))
                    {
                        string[] bffr = bfr.Split('/');
                        string[] param = bffr[1].Split(' ');
                        switch (bffr[1].ToLower())
                        {
                            case "join":
                                SendBytes("JOIN " + nick);
                                break;
                            case "auth":
                                if (haspass && !hasauth)
                                {
                                    SendBytes(pass);
                                }
                                break;
                            case "cmd":
                                SendBytes("*" + string.Join(" ", bffr.Skip(2)));
                                break;
                            default:
                                Console.WriteLine("Unknown command.");
                                goto retrycmd;
                        }
                    }
                    else
                    {
                        SendBytes(bfr);
                    }
                    clientStream.Flush();
                    
                    try
                    {
                        //blocks until a client sends a message
                        if (clientStream.CanRead)
                            bytesRead = clientStream.Read(message, 0, 4096);
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
                            Console.WriteLine("<" + msg[1] + "> " + string.Join(" ", msg.Skip(2)));
                            break;
                        default:
                            Console.WriteLine("Unknown Response From Server:");
                            Console.WriteLine(msg[0]);
                            break;
                    }
                    input = Console.ReadLine();
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
            byte[] bytes = encoder.GetBytes(nick + " " + input);
            clientStream.Write(bytes, 0, bytes.Length);
        }
    }
}
