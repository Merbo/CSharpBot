using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Security; // SSL FTW!
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

/*
 * References:
 * 
 * How to use SslStream: http://msdn.microsoft.com/library/system.net.security.sslstream(v=vs.80).aspx
 */

namespace CSharpBot
{
    public class CMDServer
    {
        // TODO: SSL implementation

        internal TcpListener _tcp;

        /// <summary>
        /// Specifies, on which port the server should listen on.
        /// </summary>
        public int Port
        { get; set; }

        /// <summary>
        /// Specifies, on which IP address the server should listen on.
        /// </summary>
        public IPAddress Address
        { get; set; }

        private bool ShouldListen = false;

        private List<CMDClient> _clients = new List<CMDClient>();
        /// <summary>
        /// A list of connected clients
        /// </summary>
        public CMDClient[] Clients
        {
            get { return _clients.ToArray(); }
        }

        public readonly char[] invalidNicknameChars = new char[] {
                                ':', ' ', '(', ')', '[', ']', 
                                '/', '\\', '"', '\'', '\0', '\a', '\r', '\n',
                                '\b'
                            };

        private Thread serverThr;

        X509Certificate serverCertificate;
        public SslProtocols UseSslProtocol
        { get; set; }

        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        public void Broadcast(string message)
        {
            foreach (CMDClient cl in _clients)
            {
                cl.Writer.WriteLine(message);
            }
        }

        /// <summary>
        /// Reads a file and sets it up as a certificate for SSL connections.
        /// </summary>
        /// <param name="file">Certificate file (X.509 CRT)</param>
        public void SetupCertificate(string file)
        {
            Console.WriteLine("[CMDServer] Loading " + file + " as a certificate...");
            serverCertificate = X509Certificate.CreateFromCertFile(file);
            if (serverCertificate == null)
                Console.WriteLine("[CMDServer] WARNING: Certificate not loaded. Please check if the file is a valid certificate!");
            else {
                Console.WriteLine("[CMDServer] Certificate has been loaded.");
                Console.WriteLine("[CMDServer] Issued by: " + serverCertificate.Issuer);
                Console.WriteLine("[CMDServer] Distinguished name: " + serverCertificate.Subject);
                Console.WriteLine("[CMDServer] Expires on: " + serverCertificate.GetExpirationDateString());
            }
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            _tcp = new TcpListener(Address, Port);
            Console.WriteLine("[CMDServer] Server starting...");
            ShouldListen = true;
            serverThr = new Thread(new ThreadStart(ProcessServer));
            serverThr.IsBackground = true;
            serverThr.Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            ShouldListen = false;
        }

        /// <summary>
        /// Indicates if the server is still listening
        /// </summary>
        public bool IsListening { get { return serverThr.IsAlive; } }

        /// <summary>
        /// Background worker function for server
        /// </summary>
        private void ProcessServer()
        {
            _tcp.Start();
            Console.WriteLine("[CMDServer] Server started.");
            DateTime n = DateTime.Now;
            while (ShouldListen)
            {
                while (!_tcp.Pending() && ShouldListen)
                {
                    Thread.Sleep(25);
                    if (DateTime.Now.Subtract(n).TotalSeconds > 30)
                    {
                        foreach (CMDClient cl in _clients)
                        {
                            try
                            {
                                if (cl.Stream.CanWrite)
                                    cl.Writer.WriteLine("PING"); // Unoptimized regular ping
                                else
                                    _clients.Remove(cl);
                            }
                            catch (SocketException)
                            {
                                _clients.Remove(cl);
                            }
                        }
                        // TODO: Implement other ping method
                        n = DateTime.Now;
                    }
                }
                if (_tcp.Pending())
                {
                    Thread clientThr = new Thread(new ParameterizedThreadStart(ProcessClient));
                    clientThr.IsBackground = true;
                    clientThr.Start(_tcp.AcceptTcpClient());
                }
            }
            Console.WriteLine("[CMDServer] Server stopping...");
            _tcp.Stop();
            Console.WriteLine("[CMDServer] Server stopped.");
        }

        /// <summary>
        /// Processes incoming data by a new client
        /// </summary>
        /// <param name="o">The client (set by thread generator)</param>
        private void ProcessClient(object o)
        {
            CMDClient thisClient = new CMDClient();
            _clients.Add(thisClient);

            TcpClient cl = o as TcpClient;
            string prefix = "[CMDServer] (" + ((IPEndPoint)cl.Client.RemoteEndPoint).Address.ToString() + ") ";
            // If it makes sense, we could implement SSL and Non-SSL connections
            // by checking the input here. But for now it makes no sense to me.
            thisClient.Stream = cl.GetStream();
            Console.WriteLine(prefix + "Client connected.");
            //SslStream ssl = new SslStream(ns, false);
            //Console.WriteLine(prefix + "SSL enabled.");
            try
            {
                /*ssl.AuthenticateAsServer(serverCertificate, false, UseSslProtocol, true);
                Console.WriteLine(prefix + "Cipher algorithm: " + ssl.CipherAlgorithm);
                Console.WriteLine(prefix + "Cipher strength: " + ssl.CipherStrength);
                Console.WriteLine(prefix + "Hash algorithm: " + ssl.HashAlgorithm);
                Console.WriteLine(prefix + "Hash strength: " + ssl.HashStrength);*/

                thisClient.Stream.ReadTimeout = 60000;
                thisClient.Stream.WriteTimeout = 60000;

                thisClient.Writer.AutoFlush = true;

                string line;
                while (cl.Connected)
                {
                    line = thisClient.Reader.ReadLine();

                    string[] split1 = line.Split(' '); // split for arguments
                    string command = split1[0].ToUpper(); // command
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

                    DateTime starttime = DateTime.Now;
                    thisClient.Writer.WriteLine("PING");

                    switch (command)
                    {
                        case "PONG":
                            Console.WriteLine(prefix + "Client is on");
                            break;
                        case "NICK":
                            Console.WriteLine(prefix + "Client identified with nickname: " + arguments[0]);
                            if (arguments[0].IndexOfAny(invalidNicknameChars) > -1)
                            {
                                thisClient.Writer.WriteLine("ERROR NICKINV :Your nickname may not contain one of these characters: " + string.Join(" ", invalidNicknameChars));
                            }
                            else
                            {
                                thisClient.Nickname = arguments[0];
                                thisClient.Writer.WriteLine("OK :Your nickname has been set.");
                            }
                            break;
                        case "MYNICK":
                            if (thisClient.Nickname != null)
                                thisClient.Writer.WriteLine("MSG Server :Your nickname is " + thisClient.Nickname);
                            else
                                thisClient.Writer.WriteLine("ERROR NICKMISS :You don't have a nickname. Pass one with NICK command.");
                            break;
                        case "PASS":
                            Console.WriteLine(prefix + "Client tried to login.");
                            thisClient.Writer.WriteLine("ERROR NOTIMPL " + command);
                            break;
                        case "AMIAUTH":
                            thisClient.Writer.WriteLine("ERROR NOTIMPL " + command);
                            break;
                        case "QUIT":
                            Console.WriteLine(prefix + "Client sent QUIT.");
                            thisClient.Writer.WriteLine("OK :You are now being disconnected by the server.");
                            cl.Close();
                            return;
                        default:
                            thisClient.Writer.WriteLine("ERROR UNKNCMD " + command);
                            break;
                    }
                }
            } catch(SocketException x) {
                if (cl.Connected)
                {
                    try
                    {
                        thisClient.Writer.WriteLine("ERROR INTERNAL :" + x.Message);
                        cl.Close();
                        Console.WriteLine(prefix + "Force-disconnected due to internal error: " + x.Message);
                    }
                    catch
                    {
                        //
                    }
                }
                Console.WriteLine(prefix + "Client disconnected.");
                _clients.Remove(thisClient);
            }
        }
    }

    /// <summary>
    /// Represents a CMDServer-connected client
    /// </summary>
    public class CMDClient
    {
        /// <summary>
        /// The nickname of the client, if set
        /// </summary>
        public string Nickname
        { get; set; }

        /// <summary>
        /// The network stream of the client
        /// </summary>
        public NetworkStream Stream
        { get; set; }

        private StreamReader _reader_cached;
        /// <summary>
        /// The StreamReader of the client
        /// </summary>
        public StreamReader Reader
        { get { if (_reader_cached == null) _reader_cached = new StreamReader(Stream); return _reader_cached; } }

        private StreamWriter _writer_cached;
        /// <summary>
        /// The StreamWriter of the client
        /// </summary>
        public StreamWriter Writer
        { get { if (_writer_cached == null) _writer_cached = new StreamWriter(Stream); return _writer_cached; } }
    }
}
