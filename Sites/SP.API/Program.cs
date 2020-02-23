using System.IO;
using System.Reflection;
using Hjson;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.API
{
    public class Program
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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog(log);
        }
    }
}