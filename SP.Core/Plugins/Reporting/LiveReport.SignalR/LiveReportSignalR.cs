using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class LiveReportSignalR : PluginBase
	{
		// Diagnostics
		private ILogger log;

		// Configuration items
		private string reportingHubUrl;

		// SignalR hub
		public HubConnection Hub { get; set; }

		// Was a reconfigure attempted on exception?
		public bool FailedReconfigure { get; set; }

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override Task<bool> Initialize(PluginOptions options)
		{
			try
			{
				// Initiate the configuration
				IConfigurationRoot config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
#if DEBUG
					.AddJsonFile("appSettings.development.json", false, true)
#else
                    .AddJsonFile("appSettings.json", false, true)
#endif
					.AddJsonFile("logSettings.json", false, true)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(LiveReportSignalR));

				// Assign config variables
				reportingHubUrl = config["reportingHubUrl"];

				// Diagnostics
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
				Hub = new HubConnectionBuilder()
					.WithUrl(reportingHubUrl)
					//.AddMessagePackProtocol()
					.Build();

				Hub.Closed += async error =>
				{
					log.Warning("Disconnected. Reconnecting...");

					await Task.Delay(new Random().Next(0, 5) * 1000);
					await Hub.StartAsync();
				};

				// Start the hub
				await Hub.StartAsync();

				// Flag to indicate reconfigure was attempted should be reset
				FailedReconfigure = false;

				return true;
			}
			catch (Exception e)
			{
				log.Error("{0}", e);
				return false;
			}
			finally
			{
				log.Information("Completed Configuration stage");
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <returns></returns>
		public override async Task<bool> AccessAttemptEvent(AccessAttempts accessAttempt)
		{
			log.Debug("Invoking AccessAttempt");

			try
			{
				await Hub.InvokeAsync("AccessAttempt", accessAttempt);
				return true;
			}
			catch (Exception e)
			{
				await RecoverInvalidState(e.Message);

				log.Error(e.Message);
				return false;
			}
		}

		/// <summary>
		/// </summary>
		public override async Task<bool> BlockEvent(Blocks block)
		{
			log.Debug("Invoking Block");

			try
			{
				await Hub.InvokeAsync("Block", block);
				return true;
			}
			catch (Exception e)
			{
				await RecoverInvalidState(e.Message);

				log.Error(e.Message);
				return false;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public override async Task<bool> UnblockEvent(Blocks block)
		{
			log.Debug("Invoking Unblock");

			try
			{
				await Hub.InvokeAsync("Unblock", block);
				return true;
			}
			catch (Exception e)
			{
				await RecoverInvalidState(e.Message);

				log.Error(e.Message);
				return false;
			}
		}

		//
		private async Task RecoverInvalidState(string exceptionMessage)
		{
			if (!FailedReconfigure && exceptionMessage.Contains("connection is not active"))
			{
				// Attempt to reconfigure
				await Configure();
			}
		}
	}
}