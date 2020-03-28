using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;


/// <summary> 
/// Author:    Allan Feller
/// Partner:   None 
/// Date:      3/29/2020 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Allan Feller - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Allan Feller, certify that I wrote this code from scratch and did not copy it in part or whole from  
/// another source.  All references used in the completion of the assignment are cited in my README file. 
/// 
/// File Contents 
/// 
///    This is a class intended to be used to conduct basic stress-tests the ChatClient and ChatServer classes and their connections
///    to each other. These tests were designed with a singular machine, and as such are all local-based tests.
///    These tests must be conducted on the command line, using the "dotnet run ..." command.
/// </summary>
namespace CS3500
{
	class StressTests
	{
		static void Main(string[] args)
		{
			int testID;

			if (args.Length == 0)
			{
				Console.WriteLine("Usage: [server] | [client] | [local #] | [remote #[");
				return;
			}

			if (args[0].Equals("server"))
			{
				ChatServer.Main(args);
				return;
			}
			else if (args[0].Equals("client"))
			{
				ChatClient.Main(args);
				return;
			}
			else if (!(args[0].Equals("local") || args[0].Equals("remote")) || !Int32.TryParse(args[1], out testID))
			{
				Console.WriteLine($"Your input is invalid somewhere: {args[0]} {args[1]}");
				Console.WriteLine("Exiting");
				return;
			}
			ServiceCollection services = new ServiceCollection();
			using (CustomFileLogProvider provider = new CustomFileLogProvider())
			{
				services.AddLogging(configure =>
				{
					configure.AddConsole();
					configure.AddProvider(provider);
					configure.SetMinimumLevel(LogLevel.Trace);
				});


				using (ServiceProvider serviceProvider = services.BuildServiceProvider())
				{
					ILogger logger = serviceProvider.GetRequiredService<ILogger<ChatServer>>();
					ILogger logger2 = serviceProvider.GetRequiredService<ILogger<StressTests>>();
					ILogger logger3 = serviceProvider.GetRequiredService<ILogger<ChatClient>>();


					switch (testID)
					{
						case 1:
							Stress_test_1(args[0].Equals("local"), logger, logger2);
							break;
						case 2:
							Stress_test_2(args[0].Equals("local"), logger, logger2, logger3);
							break;
						case 3:
							Stress_test_3(args[0].Equals("local"), logger);
							break;
						case 4:
							Stress_test_4(args[0].Equals("local"), logger);
							break;
					}
				}

			}

		}

		/// <summary>
		/// Tests a single client connecting to a server, and sending a message to that client.
		/// </summary>
		static void Stress_test_1(bool local, ILogger logger1, ILogger logger2)
		{
			if (!local)
			{
				Console.WriteLine("This test is only for a local setup");
				return;
			}

			ChatServer server = new ChatServer(logger1);
			server.StartServerTest();

			Thread.Sleep(1000);

			ChatClient client = new ChatClient(11000, logger2);
			client.ConnectToServer("127.0.0.1");

			Thread.Sleep(1000);


			server.SendMessage("Hello, world!");
		}

		/// <summary>
		/// Tests 2 clients connecting to a server, and sending a message to those clients.
		/// </summary>
		static void Stress_test_2(bool local, ILogger logger1, ILogger logger2, ILogger logger3)
		{
			if (!local)
			{
				Console.WriteLine("This test is only for a local setup");
				return;
			}

			ChatServer server = new ChatServer(logger1);
			server.StartServerTest();

			Thread.Sleep(1000);

			ChatClient client = new ChatClient(11000, logger2);
			ChatClient client2 = new ChatClient(11000, logger3);
			client.ConnectToServer("127.0.0.1");
			client2.ConnectToServer("127.0.0.1");

			Thread.Sleep(1000);


			server.SendMessage("Hello, world!");
		}

		/// <summary>
		/// Tests 20 clients connecting to a server, and sending 2 messages to those clients.
		/// </summary>
		static void Stress_test_3(bool local, ILogger logger1)
		{
			if (!local)
			{
				Console.WriteLine("This test is only for a local setup");
				return;
			}

			ChatServer server = new ChatServer(logger1);
			server.StartServerTest();

			Thread.Sleep(1000);

			for (int i = 0; i < 20; i++){
			ChatClient client = new ChatClient(11000);
			client.ConnectToServer("127.0.0.1");
			}

			Thread.Sleep(1000);


			server.SendMessage("Hello, world!");
			Thread.Sleep(3000);
			server.SendMessage("Goodbye, world!");
		}

		/// <summary>
		/// Tests 100 clients connecting to a server, and sending 2 messages to those clients.
		/// </summary>
		static void Stress_test_4(bool local, ILogger logger1)
		{
			if (!local)
			{
				Console.WriteLine("This test is only for a local setup");
				return;
			}

			ChatServer server = new ChatServer(logger1);
			server.StartServerTest();

			Thread.Sleep(1000);

			for (int i = 0; i < 100; i++)
			{
				ChatClient client = new ChatClient(11000);
				client.ConnectToServer("127.0.0.1");
			}

			Thread.Sleep(5000);


			server.SendMessage("Hello, everyone!");
			Thread.Sleep(3000);
			server.SendMessage("Goodbye, everyone!");
		}
	}
}
