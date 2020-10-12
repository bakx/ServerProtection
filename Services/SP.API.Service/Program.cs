using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
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
				.ConfigureServices((hostContext, services) => { services.AddSingleton(config); })
				.ConfigureWebHostDefaults(
					webBuilder =>
					{
						webBuilder.UseStartup<Startup>();
						webBuilder.ConfigureKestrel(options =>
						{	
							options.ConfigureHttpsDefaults(httpsOptions =>
							{
								httpsOptions.SslProtocols = SslProtocols.Tls12;


							});
							options.Listen(IPAddress.Any, 5001, listenOptions =>
							{
								// Uncomment the following to enable Nagle's algorithm for this endpoint.
								//listenOptions.NoDelay = false;
								listenOptions.UseConnectionLogging();

								listenOptions.UseHttps("C:\\Users\\gideo\\source\\repos\\Server Protect\\Services\\SP.API.Service\\SP.pfx",
									"sp");

								listenOptions.KestrelServerOptions.Limits.MaxConcurrentConnections = long.MaxValue;
								

							});
						});
					})
				.UseSerilog(log);
		}
	}
}