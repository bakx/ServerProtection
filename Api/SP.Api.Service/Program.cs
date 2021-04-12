using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.Api.Service
{
    internal static class Program
    {
        private static readonly string
            BasePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Initiate the configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(BasePath)
#if DEBUG
                .AddJsonFile("config/appSettings.development.json", false, false)
#else
                .AddJsonFile("config/appSettings.json", false, false)
#endif
                .AddJsonFile("config/logSettings.json", false, false)
                .Build();

            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            
            log.Debug($"Starting up...");

            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) => { services.AddSingleton(config); })
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseConfiguration(config);
                        webBuilder.UseStartup<Startup>();
                    })
                .UseSerilog(log);
        }
    }
}