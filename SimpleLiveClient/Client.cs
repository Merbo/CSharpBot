using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security; // SSL connections FTW :D
using System.Threading;
using Client;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class Client
    {
        public static string server, nick, pass;
        public static int port
        {
            get;

            set;
        }

        public static bool IsAuthenticated
        { get; set; }

        // Setting up socket, reader & writer   
        private static TcpClient socket;
        private static NetworkStream stream;
        //private static SslStream sslsock;
        //public static bool UseSSL = true; // Commented out since it doesn't currently make sense.
        private static StreamReader stdread;
        //private static StreamReader sslread;
        private static StreamWriter stdwrite;
        //private static StreamWriter sslwrite;

        public static StreamReader reader
        {
            get
            {
                //if (!UseSSL)
                //{
                    if (stdread == null) stdread = new StreamReader(stream);
                    return stdread;
                //}
                //else
                //{
                //    if (sslwrite == null) sslread = new StreamReader(sslsock);
                //    return sslread;
                //}
            }
        }
        public static StreamWriter writer
        {
            get
            {
                //if (!UseSSL)
                //{
                    if (stdwrite == null) stdwrite = new StreamWriter(stream);
                    return stdwrite;
                //}
                //else
                //{
                //    if (sslwrite == null) sslwrite = new StreamWriter(sslsock);
                //    return sslwrite;
                //}
            }
        }

        // There are 'shortcuts' for our functions :)
        static string read()
        {
            return Functions.read();
        }
        static string read(StreamReader reader)
        {
            return Functions.read(reader);
        }
        static void write(string data)
        {
            Functions.write(data);
        }
        static void write(string data, StreamWriter writer)
        {
            Functions.write(data, writer);
        }

        static void ReadLoop(object o)
        {
            try
            {
                while (stream.CanRead)
                {
                    string line = reader.ReadLine();

                    string[] split1 = line.Split(' '); // split for arguments
                    string replytype = split1[0].ToUpper(); // command
                    split1 = split1.Skip(1).ToArray();
                    List<string> arguments = new List<string>();
                    for (int i = 0; i < split1.Length; i++)
                    {
                        if (split1[i].StartsWith(":")) // Last argument with spaces
                        {
                            arguments.Add(string.Join(" ", split1.Skip(i)).Substring(1));
                            break;
                        }
                        else arguments.Add(split1[i]);
                    }
                    split1 = null; // unload unneeded array

                    switch (replytype)
                    {
                        case "OK":
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Success: " + arguments.Last());
                            Console.ResetColor();
                            break;

                        case "ERROR":
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Error: ");
                            string errortype = arguments[0];
                            arguments = arguments.Skip(1).ToList();
                            switch (errortype)
                            {
                                case "NICKMISS":
                                    Console.WriteLine("You need to enter your nickname first."); // This should never appear!
                                    break;
                                case "NICKINV":
                                    Console.WriteLine("Invalid nickname: " + arguments.Last());
                                    break;
                                case "AUTHFAIL":
                                    Console.WriteLine("Your login failed: " + arguments.Last());
                                    break;
                                case "AUTHMISS":
                                    Console.WriteLine("Permission denied - you need to authenticate first.");
                                    break;
                                case "LSFAIL":
                                    Console.WriteLine("LiveScript execution failed.");
                                    break;
                                case "INTERNAL":
                                    Console.WriteLine("Internal server error: " + arguments.Last());
                                    break;
                                case "NOTIMPL":
                                    Console.WriteLine("Function " + arguments[0] + " is not implemented!");
                                    break;
                                default:
                                    Console.WriteLine(string.Join(" ", arguments.ToArray()) + ")");
                                    break;
                            }
                            Console.ResetColor();
                            break;

                        case "MSG":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("<" + arguments[0] + "> " + arguments.Last());
                            break;

                        case "PING":
                            writer.WriteLine("PONG");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[read error]");
            }
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        /*
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("[SSL] Policy errors: " + sslPolicyErrors.ToString());
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("[SSL] WARNING: Policy errors: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return true; // Testing
        }
         */

        static void Main(string[] args)
        {
            Thread readThread = new Thread(new ParameterizedThreadStart(ReadLoop));
            readThread.IsBackground = true;

            tryserver:
            Console.Write("Server: ");
            server = Console.ReadLine();
            if (server == "")
                goto tryserver;

            tryport:
            Console.Write("Port: ");
            string tmp = Console.ReadLine();
            int x = 0;
            if (int.TryParse(tmp, out x))
            {
                port = x;
                if (port > 0 && port < 65535)
                {
                }
                else
                {
                    Console.WriteLine("Invalid port.");
                    goto tryport;
                }
            }
            else
            {
                Console.WriteLine("Invalid Port.");
                goto tryport;
            }

            trynick:
            Console.Write("Nick: ");
            nick = Console.ReadLine();
            if (nick == "")
                goto trynick;

            trypass:
            Console.Write("Pass (if given): ");
            pass = Console.ReadLine();
            if (pass == "")
                goto trypass;

            Console.Clear();



            try
            {
                Console.Write("Connecting... ");
                socket = new TcpClient(server, port);
                socket.ReceiveBufferSize = 1024;
                Console.WriteLine("OK! :D");


                stream = socket.GetStream();
                readThread.Start();

                // Setting up SSL
                //Console.Write("Authenticating... ");
                //try
                //{
                    //sslsock = new System.Net.Security.SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    //sslsock.AuthenticateAsClient("ICEDREAM-I5");
                    //Console.WriteLine("OK!");
                //}
                //catch
                //{
                    //Console.WriteLine("FAILED!");
                //}

                Console.Write("Logging in... ");
                write("NICK " + nick);
                if (pass != "")
                    write("PASS " + pass);
                Console.WriteLine("Data sent!");
                Console.WriteLine();
                string data = "";
                Thread.Sleep(800);
                while (!data.ToLower().Equals("/quit"))
                {
                    Thread.Sleep(200);
                    Console.Write("cmd>");
                    data = Console.ReadLine();
                    if (data.StartsWith("/") && stream.CanWrite)
                    {
                        // Why don't we just use a simple command conversion?
                        string[] arguments = data.Substring(1).Split(' '); // "/a b c d e f g" => "a","b","c","d","e","f","g"
                        string command = arguments[0].ToUpper(); // "A"
                        arguments = arguments.Skip(1).ToArray(); // "b","c","d", ...

                        switch (command)
                        {
                            case "SAY":
                                command = "MSG";
                                break;
                        }

                        string argline = string.Join(" ", arguments); // "b c d e f g"
                        write(command + " " + argline);

                        /*
                        string[] split = data.Split(' ');
                        string[] cmd = split[0].Split('/');
                        switch (cmd[1])
                        {
                            case "say":
                                write("MSG " + input);
                                break;
                            case "logout":
                                write("LOGOUT");
                                break;
                            case "help":
                                write("HELP");
                                break;
                        }
                         */
                    }
                    else
                        write("MSG " + data);
                }
                reader.Close();
                writer.Close();
                stream.Close();
            }
            catch
            {
                Console.WriteLine("Failed to connect :(");
            }
        }


    }
}