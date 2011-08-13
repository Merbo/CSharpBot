using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Linq;

namespace CSharpBot
{
    public class Clients
    {
        LiveServer LiveServer = new LiveServer();
        string[] clients;
        public void AddClient(string client)
        {
            clients[clients.Length] = client;
        }
        public void DelClient(string client)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i] == client)
                {
                    for (int x = i; x < clients.Length; x++)
                    {
                        clients[x] = clients[x + 1];    
                    }
                }
            }
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
        Clients Clients = new Clients();
        private TcpListener tcpListener;
        private Thread listenThread;
        public TcpClient client;
        LiveScript ls = new LiveScript();
        public string password;
        public ASCIIEncoding encoder = new ASCIIEncoding();

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
                //blocks until a client has connected to the server
                client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            bool hasauth = false;
            while (true)
            {
                bytesRead = 0;

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

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                Console.WriteLine(encoder.GetString(message, 0, bytesRead));
             //   Console.WriteLine(encoder.GetString(message, 0, bytesRead));
                string msg = encoder.GetString(message,0,bytesRead);
                if (hasauth && msg.StartsWith("*"))
                {
                    ls.RunScript(encoder.GetString(message, 1, bytesRead));
                }
                else
                {

                    string commandin = encoder.GetString(message, 0, bytesRead);
                    string[] command = commandin.Split(' ');
                    Console.WriteLine("LiveServer activity: " + commandin);
                    //Console.WriteLine(msg == password);
                    //Console.WriteLine(msg + "!=" + password);
                    //Console.WriteLine("Password:" + password);
                    //Console.WriteLine(password.CompareTo(commandin));
                    //Console.WriteLine(string.Compare(commandin, password));
                    //System.Windows.Forms.MessageBox.Show(msg.ToLower());
                    switch (command[0].ToLower())
                    {
                        case "join":
                            Clients.SendBytes("002 " + command[1]);
                            break;
                    }
                    if (msg == password) 
                    {
                        Console.WriteLine("Server: Client has authenticated.");
                        
                        Clients.SendBytes("003");
                        
                        hasauth = true;
                    }
                    else if (msg.Length > 0)
                    {
                        Clients.SendBytes("004 " + command[0] + " " + string.Join(" ", command.Skip(1)));
                    }
                    else if (msg.Length == 0)
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
