using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharpBot
{
   public class LiveServerClient {
        public string nick;
        public bool isAdmin;
        public TcpClient client;
        public NetworkStream stream;
        public bool isConnected;
       public string user;
       public string pass;
        public LiveServerClient()
        {
            isAdmin = false;
            isConnected = true;
        }
        
    }
    public class LiveServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;


      
    List<LiveServerClient> clients = new List<LiveServerClient>();
        ASCIIEncoding encoder = new ASCIIEncoding();
        void sendMessage(LiveServerClient client, string message)
        {
            try
            {
                client.stream.Write(encoder.GetBytes(message), 0, message.Length);
            }
            catch
            {
                client.isConnected = false;
            }
        }

        void brodcastMessage(string message)
        {
                foreach (LiveServerClient client in clients)
                {
                    sendMessage(client, message);
                }
         }

        void HasLeft(string nick)
        {
            
            foreach (LiveServerClient client in clients)
            {
               
                try
                {
                    string msg = nick + " has left";
                    byte[] sendmsg = encoder.GetBytes(msg);
                    client.stream.Write(sendmsg,0,msg.Length);
                }
                catch
                {
                    
                    clients.Remove(client);
                    break;
                }

            }
        }

        string readMessage(LiveServerClient client)
        {
            byte[] rawData = new byte[4096];
            int bytesRead = 0;
            try
            {
                //blocks until a client sends a message
                bytesRead = client.stream.Read(rawData, 0, 4096);
            }
            catch
            {
                //a socket error has occured
                //Console.ForegroundColor = ConsoleColor.Red;
                //System.Console.WriteLine("SOCKET ERROR IN LIVE SERVER!");
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine("Continuing....");
                client.isConnected = false;
            }

            if (bytesRead == 0) {
                client.isConnected = false;
            }

            return encoder.GetString(rawData, 0, bytesRead);


        }

        public LiveServer() {
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                LiveServerClient nxtcli = new LiveServerClient();
                //blocks until a client has connected to the server
                nxtcli.client = this.tcpListener.AcceptTcpClient();
                nxtcli.stream = nxtcli.client.GetStream();
                clients.Add(nxtcli);
                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(nxtcli);
            }
        }

        private void HandleClientComm(object nxtcli)
        {
            LiveServerClient client = (LiveServerClient)nxtcli;
            

            byte[] message = new byte[4096];
            string data;
            string[] splitdata;
            string sourcenick;
            while (client.isConnected)
            {
                
                data = readMessage(client);
                splitdata = data.Split(' ');
                sourcenick = splitdata[0].Split('\r')[0];

                if (client.isConnected)
                {
                    switch (splitdata[1])
                    {
                        case "NICK":
                            if (client.user == "" | client.user == null)
                            {
                                client.user = splitdata[2].Split('\r')[0];
                              
                                brodcastMessage(client.user + " has connected");
                                Console.WriteLine(client.user + " has connected");
                            }
                            else
                            {
                                //Console.WriteLine(client.user + " is now known as " + splitdata[2]);
                                client.user = splitdata[2].Split('\r')[0];
                            }
                            break;

                        default:
                            sendMessage(client,"Unknown command: " + splitdata[1]);
                            break;
                    }

                }       
                
                }

           
            HasLeft(client.user);
            
        }
    }
}
