﻿using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace CSharpBot
{

    public class LiveServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        TcpClient client;
        LiveScript ls = new LiveScript();
        public string password;
        ASCIIEncoding encoder = new ASCIIEncoding();

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
               
             //   Console.WriteLine(encoder.GetString(message, 0, bytesRead));
                if (hasauth)
                {
                    ls.RunScript(encoder.GetString(message, 0, bytesRead));
                }
                else
                {
                    if (encoder.GetString(message, 0, bytesRead) == password)
                    {
                        clientStream.Write(encoder.GetBytes("OK"), 0, 2);
                        hasauth = true;
                    }
                    else
                    {
                        clientStream.Write(encoder.GetBytes("END"), 0, 3);
                    }
                }
                clientStream.Write(encoder.GetBytes("OK"), 0, 2);
            }

           // tcpClient.Close();
        }


    }
}
