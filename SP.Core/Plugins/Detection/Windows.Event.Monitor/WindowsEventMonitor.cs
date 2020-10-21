using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Models.Enums;
using SP.Plugins;
using EventLogEntry = Plugins.Models.EventLogEntry;

namespace Plugins
{
	public class WindowsEventMonitor : PluginBase
	{
		/// <summary>
		/// </summary>
		public enum Events : long
		{
			FailedLogin = 4625
		}

		//
		private List<long> actionableEvents;

		private IConfigurationRoot config;
		private ILogger log;
		private IPluginBase.LoginAttempt loginAttemptsHandler;

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
					.ForContext(typeof(WindowsEventMonitor));

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
				// Load actionable events from the configuration
				actionableEvents = config.GetSection("ActionableEvents").Get<List<long>>();

				// Register EventLog
				EventLog eventLog = new EventLog("Security");
				eventLog.EntryWritten += EventLogOnEntryWritten;
				eventLog.EnableRaisingEvents = true;

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
		/// <param name="loginAttemptHandler"></param>
		/// <returns></returns>
		public override async Task<bool> RegisterLoginAttemptHandler(IPluginBase.LoginAttempt loginAttemptHandler)
		{
			loginAttemptsHandler = loginAttemptHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventLogOnEntryWritten(object sender, EntryWrittenEventArgs e)
		{
			// If not actionable, ignore
			if (!actionableEvents.Contains(e.Entry.InstanceId))
			{
				return;
			}

			switch (e.Entry.InstanceId)
			{
				// Failed login
				case (long) Events.FailedLogin:
				{
					// Parse entry into EventLogEntry
					EventLogEntry eventLogEntry = new EventLogEntry(e.Entry.ReplacementStrings);

					// Trigger login attempt event
					LoginAttempts loginAttempt = new LoginAttempts
					{
						IpAddress = eventLogEntry.SourceNetworkAddress,
						EventDate = DateTime.Now,
						Details = $"Repeated RDP login failures. Last user: {eventLogEntry.AccountAccountName}",
						AttackType = AttackType.BruteForce
					};

					// Log attempt
					log.Information(
						$"Workstation {eventLogEntry.SourceWorkstationName} from {eventLogEntry.SourceNetworkAddress} failed logging in.");

					// Fire event
					loginAttemptsHandler?.Invoke(loginAttempt);
					break;
				}
			}
		}
	}
}