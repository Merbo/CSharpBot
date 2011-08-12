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
        
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Server:");
                string server = Console.ReadLine();
                client.Connect(server, 3000);
                // client.Connect(serverEndPoint);
                Console.WriteLine();
                clientStream = client.GetStream();

                Console.Write('>');
                input = Console.ReadLine();
                int bytesRead;
                byte[] message = new byte[4096];
                while (input != "END")
                {

                    byte[] buffer = encoder.GetBytes(input);

                    clientStream.Write(buffer, 0, buffer.Length);
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
               
                    if (decodedMessage == "OK")
                    {
                        Console.WriteLine("OK");
                    }
                    else if (decodedMessage == "END")
                    {
                        Environment.Exit(0);
                    }
                    else { }
                    Console.Write('>');
                    input = Console.ReadLine();
                }
                client.Close();
            }
            catch (Exception ex)
            {
                Environment.Exit(0);
            }
        }
    }
}
