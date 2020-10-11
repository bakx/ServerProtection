using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.API.Service
{
	internal static class Program
	{
		private static readonly string
			BasePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

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
				.UseSerilog(log)
				.ConfigureServices((hostContext, services) => { services.AddSingleton(config); })
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}
	}
}