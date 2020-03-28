using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;


/// <summary> 
/// Authors:   Allan Feller, Jim De St. Germain
/// Partner:   None 
/// Date:      3/29/2020 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Allan Feller - This work may not be copied for use in Academic Coursework. 
/// 
/// A majority of this code-base was used with permission from Prof. Jim De St. Germain for the CS 3500 class during the
/// Spring 2020 semester at the University of Utah. I, Allan Feller, have made modifications to the code, but I hold no
/// ownership over a majority of this code-base.
/// 
/// File Contents 
/// 
///    This file includes a representation of a SocketState as well as a ChatClient, a basic chat client with can recieve messages
///    from a basic ChatServer. The ChatClient class is also capable of logging information to its local directory.
/// </summary>
namespace CS3500
{

    class SocketState
    {
        public Socket theSocket;
        public byte[] messageBuffer;
        public StringBuilder sb;

        public SocketState(Socket s)
        {
            theSocket = s;
            messageBuffer = new byte[1024];
            sb = new StringBuilder();
        }
    }

    /// <summary>
    /// A representation of a simple chat client that connects to a server to recieve messages
    /// </summary>
    public class ChatClient
    {
        /// <summary>
        /// The port that this client is connected to
        /// </summary>
        private int port = -1;

        /// <summary>
        /// The logger to use when logging information
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates a client at the given port with no logger.
        /// </summary>
        /// <param name="port"></param>
        public ChatClient(int port){
            this.port = port;
        }

        /// <summary>
        /// Creates a client at the given port with the given logger
        /// </summary>
        /// <param name="port"></param>
        /// <param name="Logger"></param>
        public ChatClient(int port, ILogger Logger){
            this.port = port;
            this.logger = Logger;
        }

        public static void Main(string[] args)
        {
            ChatClient client;

            Console.WriteLine("enter server address:");
            string serverAddr = Console.ReadLine();

            Console.WriteLine("enter server port:");
            string portNumber = Console.ReadLine();

            ServiceCollection services = new ServiceCollection();


            if (!Int32.TryParse(portNumber, out int port))
            {
                Console.WriteLine("could not understand that... exiting");
            }
            else
            {
                try
                {
                    // Set up the logger to use
                    using (CustomFileLogProvider provider = new CustomFileLogProvider())
                    {
                        services.AddLogging(configure =>
                        {
                            configure.AddConsole();
                            configure.AddProvider(provider);
                            configure.SetMinimumLevel(LogLevel.Information);
                        });


                        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                        {
                            ILogger<ChatClient> logger = serviceProvider.GetRequiredService<ILogger<ChatClient>>();
                            client = new ChatClient(port, logger);
                            client.ConnectToServer(serverAddr);


                            // Hold the console open
                            Console.WriteLine("Press Enter to Exit");
                            Console.Read();
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in connection {e}");
                }
            }
        }

        /// <summary>
        /// Starts the connection process
        /// </summary>
        /// <param name="serverAddr"></param>
        public void ConnectToServer(string serverAddr)
        {
            // Parse the IP
            IPAddress addr = IPAddress.Parse(serverAddr);

            // Create a socket
            Socket s1 = new Socket(addr.AddressFamily, SocketType.Stream,
              ProtocolType.Tcp);
            //// Create a socket
            //Socket s2 = new Socket(addr.AddressFamily, SocketType.Stream,
            //  ProtocolType.Tcp);

            SocketState ss1 = new SocketState(s1);
            //SocketState ss2 = new SocketState(s2);

            // Connect
            ss1.theSocket.BeginConnect(addr, port, OnConnected, ss1);
            //ss2.theSocket.BeginConnect(addr, port, OnConnected, ss2);
        }

        /// <summary>
        /// Callback for when a connection is made (see line 62)
        /// Finalizes the connection, then starts a receive loop.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnected(IAsyncResult ar)
        {
            Console.WriteLine("Was able to contact the server and establish a connection");
            logger?.LogInformation($"Client {port} connected to server successfully");

            SocketState theServer = (SocketState)ar.AsyncState;

            // this does not end the connection! this simply acknowledges that we are at the _end_ of the start
            // of the connection phase!
            theServer.theSocket.EndConnect(ar);

            // Start a receive operation
            theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None,
                OnReceive, theServer);
        }


        /// <summary>
        /// Callback for when a receive operation completes (see BeginReceive)
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            Console.WriteLine("On Receive callback executing. ");
            SocketState theServer = (SocketState)ar.AsyncState;
            int numBytes = theServer.theSocket.EndReceive(ar);
            logger?.LogDebug($"Recieved exactly {numBytes} bytes of data from the server");
            string message = Encoding.UTF8.GetString(theServer.messageBuffer, 0, numBytes);
            Console.WriteLine($"   Received {message.Length} characters.  Could be a message (or not) based on protocol");
            Console.WriteLine($"     Data is: {message}");

            theServer.sb.Append(message);

            ProcessMessages(theServer.sb);

            // Continue the "event loop" and receive more data
            theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None,
                OnReceive, theServer);
        }


        /// <summary>
        /// Look for complete messages (terminated by a '.'), 
        /// then print and remove them from the string builder.
        /// </summary>
        /// <param name="sb"></param>
        private void ProcessMessages(StringBuilder sb)
        {
            string totalData = sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\.])");

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // Ignore last message if incomplete
                if (p[p.Length - 1] != '.')
                    break;

                // process p
                Console.WriteLine("message received");
                Console.WriteLine(p);

                sb.Remove(0, p.Length);

            }
        }

    }
}
