using System;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Linq;

namespace CSharpBot
{
    public class Clients
    {
        internal LiveServer LiveServer;

        public Clients(LiveServer theserver)
        {
            this.LiveServer = theserver;
        }

        public List<string> clients = new List<string>();
        public List<string> Admins = new List<string>();

        public void AddClient(string client)
        {
            clients.Add(client);
        }
        public void DelClient(string client)
        {
            clients.Remove(client);
        }
        public void AddAdmin(string admin)
        {
            Admins.Add(admin);
        }
        public void DelAdmin(string admin)
        {
            Admins.Remove(admin);
        }
        public void SendBytes(string input)
        {
            TcpClient tcpClient = (TcpClient)LiveServer.client;
            NetworkStream clientStream = tcpClient.GetStream();
            clientStream.Write(LiveServer.encoder.GetBytes(input), 0, input.Length);
        }
    }

    public class LiveServer
    {
        Clients Clients;
        private TcpListener tcpListener;
        private Thread listenThread;
        internal TcpClient client;
        LiveScript ls = new LiveScript();
        public string Password;

        public ASCIIEncoding encoder = new ASCIIEncoding();

        public LiveServer() {
            this.Clients = new Clients(this);
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {

            this.tcpListener.Stop(); // Stop if active
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.IsBackground = true; // make it work in background, so other clients may connect, too.
                clientThread.Start(client);
            }
        }
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (System.IO.IOException)
                {
                    //Do nothing, a user disconnected from the server.    
                }
                catch (Exception e)
                {
                    //a socket error has occured
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(e.ToString() + "\r\n");
                    Console.ResetColor();
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }
                //message has successfully been received
                if (CSharpBot.bot.DebuggingEnabled)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Server Activity: " + encoder.GetString(message, 0, bytesRead));
                    Console.ResetColor();
                }
                    
                //Console.WriteLine(encoder.GetString(message, 0, bytesRead));
                string msg = encoder.GetString(message, 0, bytesRead);
                string[] cmd = msg.Split(' ');
                if (cmd.Length > 1)
                {
                    if (Clients.Admins.Contains(cmd[0].ToString()) && cmd[1].ToString().StartsWith("*"))
                    {
                        ls.RunScript(encoder.GetString(message, cmd[0].ToString().Length + 1, bytesRead));
                    }
                }
                else
                {

                    string commandin = encoder.GetString(message, 0, bytesRead);
                    string[] command = commandin.Split(' ');
                    Console.WriteLine("LiveServer activity: " + commandin);
                    switch (command[1].ToLower())
                    {
                        case "join":
                            Clients.SendBytes("002 " + command[1]);
                            break;
                        case "pass":
                            Console.WriteLine("Server: Client has authenticated.");
                            Clients.SendBytes("003");
                            Clients.AddAdmin(command[0]);
                            break;
                    }
                    if (command.Length > 2)
                    {
                        Clients.SendBytes("004 " + command[0] + " " + string.Join(" ", command.Skip(1)));
                    }
                    else if (command.Length == 0)
                    {
                        Clients.SendBytes("000");
                    }
                    else
                    {
                        Clients.SendBytes("001");
                    }
                }
                
            }

           // tcpClient.Close();
        }


    }
}
