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

        public static void SendBytes(string input)
        {
            //clientStream.Write(bfer, 0, bfer.Length);
            byte[] cmd = System.Text.Encoding.ASCII.GetBytes(input);
            clientStream.Write(cmd, 0, cmd.Length);
        }
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Server:");
                string server = Console.ReadLine();
                Console.Write("Nick:");
                string nick = Console.ReadLine();
                Console.Write("Pass:");
                string pass = Console.ReadLine();
                client.Connect(server, 3000);
                // client.Connect(serverEndPoint);
                Console.WriteLine();
                clientStream = client.GetStream();

                input = Console.ReadLine();
                int bytesRead;
                byte[] message = new byte[4096];
                while (input.ToLower() != "/quit")
                {

                    byte[] buffer = encoder.GetBytes(input);
                    string bfr = buffer.ToString();
                    if (bfr.StartsWith("/"))
                    {
                        string[] bffr = bfr.Split('/');
                        string[] param = bffr[1].Split(' ');
                        switch (bffr[1].ToLower())
                        {
                            case "join":
                                SendBytes("JOIN " + nick);
                                break;
                            case "cmd":
                                SendBytes("*" + string.Join(" ", bffr.Skip(2)));
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
