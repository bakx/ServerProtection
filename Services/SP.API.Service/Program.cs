using System.IO;
using System.Reflection;
using Hjson;
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
#if DEBUG
			HjsonValue.Save(HjsonValue.Load(Path.Combine(BasePath, "config/appSettings.development.hjson")).Qo(),
				"config/appSettings.development.json");
#else
            HjsonValue.Save(HjsonValue.Load(Path.Combine(BasePath, "config/appSettings.hjson")).Qo(), "config/appSettings.json");
#endif

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
				.ConfigureServices((hostContext, services) =>
				{
					services.AddSingleton(config);
					services.AddHostedService<ApiService>();
				})
				.UseSerilog(log);
		}
	}
}