using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class LoadSimulator : PluginBase
	{
		private IConfigurationRoot config;
		private ILogger log;

		private IPluginBase.AccessAttempt accessAttemptsHandler;

		//
		private int parallelThreads;
		private Timer timer;

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override Task<bool> Initialize(PluginOptions options)
		{
			try
			{
				// Initiate the configuration
				config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
					.AddJsonFile("appSettings.json", false, true)
					.AddJsonFile("logSettings.json", false, true)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(LoadSimulator));

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
			try
			{
				// Load configuration
				parallelThreads = config.GetSection("ParallelThreads").Get<int>();

				timer = new Timer(Callback, null, TimeSpan.FromSeconds(5), TimeSpan.Zero);

				return await Task.FromResult(true);
			}
			catch (Exception e)
			{
				log.Error("{0}", e);
				return await Task.FromResult(false);
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

			accessAttemptsHandler = accessAttemptHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// </summary>
		/// <param name="state"></param>
		private void Callback(object state)
		{
			// Disable timer
			timer.Change(0, 1000);

			Parallel.For((long) 0, parallelThreads, (i, res) =>
			{
				// Trigger login attempt event
				AccessAttempts accessAttempt = new AccessAttempts
				{
					IpAddress = $"123.123.123.{i}",
					EventDate = DateTime.Now,
					Details = "Repeated RDP login failures. Last user: loadTest"
				};

				// Log attempt
				log.Information(
					"Load test simulation.");

				// Fire event
				accessAttemptsHandler?.Invoke(accessAttempt);
			});
		}
	}
}