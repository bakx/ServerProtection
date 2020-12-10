using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Plugins;

namespace Plugins
{
	public class LinuxLog : PluginBase
	{
		private IConfigurationRoot config;
		private ILogger log;

		private readonly List<Monitor> monitors = new List<Monitor>();
        private List<ConfigurationItem> detectionConfig;

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override Task<bool> Initialize(PluginOptions options)
		{
			try
			{
				string basePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName;

				if (basePath == null)
				{
					log.Warning("Unable to retrieve base path. Configuration might not load correctly.");
				}

				// Initiate the configuration
				config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName)
#if DEBUG
					.AddJsonFile("appSettings.development.json", false)
#else
                    .AddJsonFile("appSettings.json", false)
#endif
					.AddJsonFile("logSettings.json", false)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(LinuxLog));

				log.Information("Plugin initialized");

				return Task.FromResult(true);
			}
			catch (Exception e)
			{
				if (log == null)
				{
					Console.WriteLine(e);
				}
				else
				{
					log.Error(e.Message);
				}

				return Task.FromResult(false);
			}
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override async Task<bool> Configure()
		{
			// Get configuration items
            detectionConfig = config.GetSection("Detection").Get<List<ConfigurationItem>>();

			try
            {
                foreach (ConfigurationItem configItem in detectionConfig)
                {
                    Monitor monitor = new Monitor(log, configItem);
                    monitors.Add(monitor);
                }

                return await Task.FromResult(true);
            }
			finally
			{
				if (log == null)
				{
					Console.WriteLine("Completed Configuration stage");
				}
				else
				{
					log.Information("Completed Configuration stage");
				}
			}
		}

		/// <summary>
		/// Register the LoginAttemptsHandler in order to fire events
		/// </summary>
		/// <param name="accessAttemptHandler"></param>
		/// <returns></returns>
		public override async Task<bool> RegisterAccessAttemptHandler(IPluginBase.AccessAttempt accessAttemptHandler)
		{
			log.Debug($"Registered handler: {nameof(RegisterAccessAttemptHandler)}");

			// Pass the handler to the listening monitors
            foreach (Monitor monitor in monitors)
            {
				monitor.RegisterAccessAttemptsHandler(accessAttemptHandler);
				monitor.Start();
            }

			return await Task.FromResult(true);
		}
	}
}