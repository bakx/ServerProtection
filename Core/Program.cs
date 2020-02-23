using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Hjson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SP.Core.Interfaces;

namespace SP.Core
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            HjsonValue.Save(HjsonValue.Load("config/appSettings.development.hjson").Qo(),
                "config/appSettings.development.json");
#else
            HjsonValue.Save(HjsonValue.Load("config/appSettings.hjson").Qo(), "config/appSettings.json");
#endif

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Initiate the configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
#if DEBUG
                .AddJsonFile("config/appSettings.development.json", false, true)
#else
                .AddJsonFile("config/appSettings.json", false, true)
#endif
                .AddJsonFile("config/logSettings.json", false, true)
                .Build();

            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(config);
                    services.AddSingleton<IProtectHandler, ProtectHandler>();
                    services.AddSingleton<IApiHandler, ApiHandler>();
                    services.AddSingleton<IFirewall, Firewall>();

                    HttpClient httpClient = new HttpClient
                    {
                        BaseAddress = new Uri(config["Api:Url"])
                    };

                    httpClient.DefaultRequestHeaders.Add("token", config["Api:Token"]);

                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    services.AddSingleton(httpClient);

                    services.AddHostedService<CoreService>();
                })
                .UseSerilog(log);
        }
    }
}