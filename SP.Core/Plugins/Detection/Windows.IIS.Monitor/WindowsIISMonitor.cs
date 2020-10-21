using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Models.Enums;
using SP.Plugins;

namespace Plugins
{
	public class WindowsIISMonitor : PluginBase
	{
		private List<string> sitesMonitored;
		private List<string> exploitPaths;

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
					.ForContext(typeof(WindowsIISMonitor));

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
				sitesMonitored = config.GetSection("SitesMonitored").Get<List<string>>();
				exploitPaths = config.GetSection("ExploitPaths").Get<List<string>>();

				using TraceEventSession session = new TraceEventSession("IIS-ETW");

				// Enable IIS ETW provider and set up a new trace source on it
				session.EnableProvider(IISLogTraceEventParser.ProviderName);

				using ETWTraceEventSource traceSource = new ETWTraceEventSource("IIS-ETW", TraceEventSourceType.Session);
				IISLogTraceEventParser parser = new IISLogTraceEventParser(traceSource);

				// Listen to event
				parser.IisLog += ParserOnIisLog;

				// Start processing
				traceSource.Process();
				// traceSource.StopProcessing();

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
		/// 
		/// </summary>
		/// <param name="data"></param>
		private void ParserOnIisLog(IISLogTraceData data)
		{
			// Check if this site is monitored
			if (!sitesMonitored.Contains(data.ServiceName))
			{
				return;
			}

			// Check if path is in prohibited list
			if (!exploitPaths.Contains(data.UriStem))
			{
				return;
			}

			// Trigger login attempt event
			LoginAttempts loginAttempt = new LoginAttempts
			{
				IpAddress = data.ClientIp,
				EventDate = DateTime.Now,
				Details = $"Attempt to access {data.UriStem} by IP {data.ClientIp}",
				OverrideBlock = true,
				AttackType = AttackType.WebExploit
			};

			// Log attempt
			log.Information(
				$"Attempt to access {data.UriStem} on {data.ServiceName} by IP {data.ClientIp}.");

			// Fire event
			loginAttemptsHandler?.Invoke(loginAttempt);
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
	}
}