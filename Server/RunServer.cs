using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CS3500
{
	class RunServer
	{

        static void Main(string[] args)
        {

            ChatServer server;

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
                    ILogger<ChatServer> logger = serviceProvider.GetRequiredService<ILogger<ChatServer>>();
                    server = new ChatServer(logger);
                    server.StartServer();


                    //Thread.Sleep(10000);
                }
            }
        }


    }
}
