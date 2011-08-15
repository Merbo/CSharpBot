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

            Console.WriteLine("[LiveServer] Now listening.");
            
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

            Console.WriteLine("[LiveServer] Client connected: " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());

            byte[] message = new byte[4096];
            int bytesRead;
            string nickname = null;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
#if DEBUG
                    Console.WriteLine("[LiveServer] (" + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString() + ") Received " + bytesRead.ToString() + " bytes");
#endif
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
                    Console.WriteLine("[LiveServer] (" + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString() + ") Disconnected.");
                    return;
                }
                //message has successfully been received
                string msg = encoder.GetString(message, 0, bytesRead);
                
                if (CSharpBot.bot.DebuggingEnabled)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[LiveServer] Activity: " + msg);
                    Console.ResetColor();
                }

                //Console.WriteLine(encoder.GetString(message, 0, bytesRead));
                string[] cmdsplit = msg.Split(' ');

                string command = cmdsplit[0].ToLower();
                string[] arguments = cmdsplit.Skip(1).ToArray();
                string argline = string.Join(" ", arguments);

                if(command.StartsWith("*"))
                {
                    if(nickname != null)
                    {
                        if (Clients.Admins.Contains(nickname))
                            {
                                Console.WriteLine("[LiveServer] Running remote-control script.");
                                try {
                                    ls.RunScript(command.Substring(1) + " " + argline);
                                    Clients.SendBytes("020");
                                } catch(Exception) {
                                    Clients.SendBytes("021");
                                }
                            } else {
                                Clients.SendBytes("012");
                            }
                    } else Clients.SendBytes("009");
                }

                switch(command.ToLower())
                {
                    case "nick":
                        if(arguments.Length < 1)
                            Clients.SendBytes("006");
                        else
                        {
                            nickname = arguments[0];
                            Clients.SendBytes("005");
                        }
                        break;

                    case "pass":
                        if(nickname == null)
                            Clients.SendBytes("009");
                        else if(arguments.Length < 1)
                            Clients.SendBytes("003");
                        else if(arguments[0] != this.Password)
                            Clients.SendBytes("010");
                        else
                        {
                            Console.WriteLine("[LiveServer] " + nickname + " has authenticated as an Admin.");
                            Clients.SendBytes("003");
                            Clients.AddAdmin(arguments[0]);
                        }
                        break;

                    case "join":
                        if(nickname == null)
                            Clients.SendBytes("009");
                        else
                            Clients.SendBytes("002 " + nickname);
                        break;

                    case "msg":
                        Clients.SendBytes("004 " + nickname + " " + argline);
                        break;

                    default:
                        Clients.SendBytes("011");
                        break;
                }

                /*
#if DEBUG
                Console.WriteLine("[LiveServer] Splitting message by ' ' results in an array with " + cmd.Count() + " entries");
#endif
                if (cmd.Length > 1)
                {
#if DEBUG
                    Console.WriteLine("[LiveServer] if(cmd.Length > 1) is true");
                    Console.WriteLine("[LiveServer] " + cmd[0] + " is " + (Clients.Admins.Contains(cmd[0].ToString()) ? "" : "not ") + "an Admin.");
                    Console.WriteLine("[LiveServer] " + cmd[1] + " does " + (cmd[1].ToString().StartsWith("*") ? "" : "not ") + "start with *");
#endif
                    if (Clients.Admins.Contains(cmd[0].ToString()) && cmd[1].ToString().StartsWith("*"))
                    {
                        Console.WriteLine("[LiveServer] Running remote-control script.");
                        ls.RunScript(encoder.GetString(message, cmd[0].ToString().Length + 1, bytesRead));
                    } else {
                        Console.WriteLine("[LiveServer] Nothing is done, because Client is not an Admin OR this is not a livescript.");
                    }
                }
                else
                {
                    Console.WriteLine("[LiveServer] if(cmd.Length > 1) is false");
                    string commandin = encoder.GetString(message, 0, bytesRead);
                    string[] command = commandin.Split(' ');
                    Console.WriteLine("[LiveServer] activity: " + commandin);
                    switch (command[1].ToLower())
                    {
                        case "join":
                            Console.WriteLine("[LiveServer] JOIN received - sending 002");
                            Clients.SendBytes("002 " + command[1]);
                            break;
                        case "pass":
                            Console.WriteLine("[LiveServer] PASS received - sending 003");
                            Console.WriteLine("Server: Client has authenticated.");
                            Clients.SendBytes("003");
                            Clients.AddAdmin(command[0]);
                            break;
                        default:
                            Console.WriteLine("[LiveServer] Couldn't recognize command \"" + command[1] + "\"");
                            break;
                    }
                    if (command.Length > 2)
                    {
                        Console.WriteLine("[LiveServer] command.Length > 2 => Sending 004");
                        Clients.SendBytes("004 " + command[0] + " " + string.Join(" ", command.Skip(1)));
                    }
                    else if (command.Length == 0)
                    {
                        Console.WriteLine("[LiveServer] command.Length == 0 => Sending 000");
                        Clients.SendBytes("000");
                    }
                    else
                    {
                        Console.WriteLine("[LiveServer] command.Length > 2 AND command.Length == 0 are false => Sending 001");
                        Clients.SendBytes("001");
                    }
                }
                 */

            }

           // tcpClient.Close();
        }


    }
}
