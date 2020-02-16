using System.IO;
using System.Reflection;
using Hjson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.Core
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            HjsonValue.Save(HjsonValue.Load("appSettings.hjson").Qo(), "appSettings.json");
#endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Initiate the configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                .AddJsonFile("appSettings.json", false, true)
                .AddJsonFile("logSettings.json", false, true)
                .Build();

            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(config);
                    services.AddHostedService<CoreService>();
                    services.AddLogging(loggingBuilder =>
                        loggingBuilder.AddSerilog(log));
                });
        }
    }
}