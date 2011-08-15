using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
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
        //There are 'shortcuts' for our functions :)
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

        static void Main(string[] args)
        {
            tryserver:
            Console.Write("Server: ");
            server = Console.ReadLine();
            if (server == "")
                goto tryserver;
            tryport:
            Console.Write("Port: ");
            string tmp = Console.ReadLine();
            if (int.TryParse(tmp, out port))
            {
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
            try
            {
                socket = new TcpClient(server, port);
                socket.ReceiveBufferSize = 1024;
                Console.WriteLine("Connected :D");
                NetworkStream stream = socket.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                write("NICK " + nick);
                if (pass != "")
                    write("PASS " + pass);
                string input = Console.ReadLine().ToLower();
                while (!input.Equals("/quit"))
                {
                    string data = read(reader);
                    if (data.StartsWith("/"))
                    {
                        string[] split = data.Split(' ');
                        string[] cmd = split[0].Split('/');
                        switch (cmd[1])
                        {
                            case "say":
                                write("MSG " + input);
                                break;
                        }
                    }
                    else
                        write("MSG " + input);
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