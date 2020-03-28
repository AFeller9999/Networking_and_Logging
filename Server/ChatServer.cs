using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
///    This file includes a representation of a basic chat server in ChatServer, where clients can connect and recieve
///    messages and data from the server. The ChatServer class is also capable of logging information to its local directory.
/// </summary>
namespace CS3500
{


	/// <summary>
	/// A simple server for sending simple text messages to multiple clients
	/// </summary>
	public class ChatServer
	{
		/// <summary>
		/// keep track of how big a message to send... keep getting bigger!
		/// </summary>
		private long larger = 5000;

		/// <summary>
		/// The logger to use when logging information
		/// </summary>
		private ILogger sLogger;

		/// <summary>
		/// How many messages have successfully been sent
		/// </summary>
		private int successfullySentMessages = 0;


		/// <summary>
		/// A list of all clients currently connected
		/// </summary>
		private List<Socket> clients = new List<Socket>();

		private TcpListener listener;

		/// <summary>
		/// How many clients are connected to this server in total
		/// </summary>
		private int totalConnectedClients;

		/// <summary>
		///   Constructs a chat server with the given file logger.
		/// </summary>
		public ChatServer(ILogger logger)
		{
			this.sLogger = logger;
		}

		public static void Main(string[] args)
		{

			ChatServer server;

			ServiceCollection services = new ServiceCollection();

			using (CustomFileLogProvider provider = new CustomFileLogProvider())
			{
				services.AddLogging(configure =>
				{
					configure.AddConsole();
					configure.AddProvider(provider);
					configure.SetMinimumLevel(LogLevel.Debug);
				});


				using (ServiceProvider serviceProvider = services.BuildServiceProvider())
				{
					ILogger<ChatServer> logger = serviceProvider.GetRequiredService<ILogger<ChatServer>>();
					server = new ChatServer(logger);
					server.StartServer();


					//Thread.Sleep(10000);
				}
			}
		}


		/// <summary>
		/// A test variant of the StartServer() method that always starts a server at port 11000 (the default.)
		/// It also does not way for the user to input a message
		/// </summary>
		public void StartServerTest()
		{

			int port;

			if (!Int32.TryParse("11000", out port))
			{
				port = 11000;
				sLogger?.LogDebug("No port given or invalid port number, using default port instead");
			}
			else
			{
				sLogger?.LogDebug($"Starting server with port {port}");
			}

			listener = new TcpListener(IPAddress.Any, port);
			Console.WriteLine($"Server waiting for clients here: 127.0.0.1 on port {port}");

			listener.Start();

			listener.BeginAcceptSocket(ConnectionRequested, null);
		}

		/// <summary>
		/// Start accepting Tcp socket connections from clients
		/// </summary>
		public void StartServer()
		{
			Console.WriteLine("Enter port to listen on (default 11000):");
			string portStr = Console.ReadLine();

			int port;

			if (!Int32.TryParse(portStr, out port))
			{
				port = 11000;
				sLogger?.LogDebug("No port given or invalid port number, using default port instead");
			}
			else
			{
				sLogger?.LogDebug($"Starting server with port {port}");
			}

			listener = new TcpListener(IPAddress.Any, port);
			Console.WriteLine($"Server waiting for clients here: 127.0.0.1 on port {port}");

			listener.Start();

			listener.BeginAcceptSocket(ConnectionRequested, null);

			// waits for the user to type messages, then sends them
			SendMessage();
		}

		/// <summary>
		/// A callback for invoking when a socket connection is accepted
		/// </summary>
		/// <param name="ar"></param>
		private void ConnectionRequested(IAsyncResult ar)
		{
			Console.WriteLine("Contact from client");
			sLogger?.LogDebug($"Contact from client");
			// Get the socket
			clients.Add(listener.EndAcceptSocket(ar));
			totalConnectedClients++;
			sLogger?.LogDebug($"Total connected clients: {totalConnectedClients}");

			// continue an event-loop that will allow more clients to connect
			listener.BeginAcceptSocket(ConnectionRequested, null);
		}

		/// <summary>
		/// Sends a specific message across the server.
		/// </summary>
		/// <param name="message"></param>
		public void SendMessage(string message)
		{
			if (message == "largemessage")
			{
				message = GenerateLargeMessage();
				sLogger.LogDebug("Sending a large message...");
			}

			//
			// Begin sending the message
			//
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);
			List<Socket> toRemove = new List<Socket>();

			sLogger?.LogInformation($"   Sending a message of size: {message.Length}");

			foreach (Socket s in clients)
			{
				try
				{
					s.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, s);
					sLogger?.LogInformation($"Sent message to client {s}");
				}
				catch (Exception) // Begin Send fails if client is closed
				{
					sLogger?.LogInformation($"Client {s} is closed, removing them from the server");
					toRemove.Add(s);
				}
			}

			// update list of "current" clients by removing closed clients
			foreach (Socket s in toRemove)
			{
				Console.WriteLine($"Client {s} disconnected");
				clients.Remove(s);
				totalConnectedClients--;
				sLogger?.LogDebug($"Client {s} has disconnected. Total number of connected clients: {totalConnectedClients}");
			}
		}

		/// <summary>
		/// Continuously ask the user for a message to send to the client
		/// </summary>
		private void SendMessage()
		{
			while (true)
			{
				Console.WriteLine("enter a message to send");
				string message = Console.ReadLine();
				SendMessage(message);
			}
		}

		/// <summary>
		/// Generate a big string of the letter a repeated...
		/// </summary>
		/// <returns></returns>
		private string GenerateLargeMessage()
		{
			StringBuilder retval = new StringBuilder();

			for (int i = 0; i < larger; i++)
				retval.Append("a");
			retval.Append(".");

			larger += larger;

			return retval.ToString();
		}


		/// <summary>
		/// A callback invoked when a send operation completes
		/// </summary>
		/// <param name="ar"></param>
		private void SendCallback(IAsyncResult ar)
		{
			// Nothing much to do here, just conclude the send operation so the socket is happy.
			// 
			//   This code could be useful to update the state of a program once you know a client
			//   has received some information, for example, if you had a counter of successfully sent 
			//   messages you would increment it here.
			//
			Socket client = (Socket)ar.AsyncState;
			long send_length = client.EndSend(ar);
			successfullySentMessages++;
			sLogger?.LogInformation($"   Sent a message of size: {send_length}");
			sLogger?.LogDebug($"Current number of successful messages sent: {successfullySentMessages}");
		}

	}
}