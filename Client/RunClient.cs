using CS3500;
using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CS3500
{
	public static class RunClient
	{

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
    }
}
