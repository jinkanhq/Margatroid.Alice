using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Margatroid.Alice.Native;
using System.Net;
using System;

namespace Margatroid.Alice
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    if (args.Length > 0)
                    {
                        switch (args[0])
                        {
                            case "server":
                                services.AddHostedService<Server>();
                                return;
                            case "client":
                                services.AddHostedService<Client>();
                                return;
                        }
                    }
                    
                })
                .ConfigureLogging((hostContext, loggerBuilder) =>
                {
                    loggerBuilder.AddConfiguration(hostContext.Configuration);
                    loggerBuilder.AddSentry();
                });
    }
}