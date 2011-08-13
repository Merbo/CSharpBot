﻿using System;
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

        public static void SendBytes(string input)
        {
            Console.WriteLine(input);
            //clientStream.Write(bfer, 0, bfer.Length);
            byte[] cmd = encoder.GetBytes(input);
            Console.WriteLine(encoder.GetString(cmd,0,cmd.Length));
            clientStream.Write(cmd, 0, cmd.Length);
        }
        static void Main(string[] args)
        {
            try
            {
                bool hasauth = false;
                bool haspass = false;
                retryserver:
                Console.Write("Server:");
                string server = Console.ReadLine();
                if (server == "")
                    goto retryserver;
                retrynick:
                Console.Write("Nick:");
                string nick = Console.ReadLine();
                if (nick == "")
                    goto retrynick;
                
                Console.Write("Password(If any):");
                string pass = Console.ReadLine();
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
                                System.Console.WriteLine("Unknown command.");
                                goto retrycmd;
                                break;
                        }
                    }
                    else
                    {
                        
                        SendBytes(bfr);
                    }
                    clientStream.Flush();
                    //Console.WriteLine("OK");
                    
                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        //a socket error has occured
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

                        default:
                            Console.WriteLine("Unknown Responce From Server:");
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
    }
}