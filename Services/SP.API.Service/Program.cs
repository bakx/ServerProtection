using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

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

			// Assign config variables
			string serverHost = config.GetSection("Host").Get<string>();
			int serverPort = config.GetSection("Port").Get<int>();
			string certificatePath = config.GetSection("CertificatePath").Get<string>();
			string certificatePassword = config.GetSection("CertificatePassword").Get<string>();

			// Load certificate
			X509Certificate2 x509Certificate2 = new X509Certificate2(certificatePath, certificatePassword);

			return Host.CreateDefaultBuilder(args)
				.UseWindowsService()
				.ConfigureServices((hostContext, services) => { services.AddSingleton(config); })
				.ConfigureWebHostDefaults(
					webBuilder =>
					{
						webBuilder
							.UseKestrel()
							.UseConfiguration(config)
							//.UseUrls($"{serverHost}:{serverPort}")
							.ConfigureKestrel(serverOptions =>
							{
								serverOptions.Limits.MaxConcurrentConnections = long.MaxValue;
								serverOptions.Limits.KeepAliveTimeout = new TimeSpan(0, 0, 0, 5);

								serverOptions.ConfigureHttpsDefaults(listenOptions =>
								{
									listenOptions.ServerCertificate = x509Certificate2;
								});
							})
							.UseStartup<Startup>();
					});
			//.UseSerilog(log);
		}
	}
}