using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Models.Enums;
using SP.Plugins;

namespace Plugins
{
	public class LinuxMailLog : PluginBase
	{
		private Thread thread;

		private IConfigurationRoot config;
		private ILogger log;
		private IPluginBase.AccessAttempt accessAttemptsHandler;

        private string logLocation;
        private string postfixRegularExpression;

        private Regex postfixFailedAuthentication;

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
					.AddJsonFile("appSettings.development.json", false, true)
#else
                    .AddJsonFile("appSettings.json", false, true)
#endif
					.AddJsonFile("logSettings.json", false, true)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(LinuxMailLog));

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
            logLocation = config.GetSection("LogLocation").Get<string>();
            postfixRegularExpression = config.GetSection("PostfixRegularExpression").Get<string>();

			postfixFailedAuthentication = new Regex(postfixRegularExpression, RegexOptions.Compiled);

			try
			{
				thread = new Thread(async () =>
				{
					// setfacl -m user:grepuser:r /var/log/maillog*

					Process process = new Process();

					process.OutputDataReceived += AnalyzeEntry;
					process.ErrorDataReceived += AnalyzeEntry;

					process.StartInfo.UseShellExecute = false;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;

					process.StartInfo.FileName = "tail";
					process.StartInfo.Arguments = $"-f {logLocation}";

					process.Start();
					process.BeginOutputReadLine();
					await process.WaitForExitAsync();

                    process.Exited += async (sender, args) =>
                    {
						log.Warning("Process exited, restarting...");

                        process.Start();
                        process.BeginOutputReadLine();
                        await process.WaitForExitAsync();
                    };

				})
				{
					IsBackground = true
				};
				thread.Start();

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

		///
		private void AnalyzeEntry(object sendingProcess, DataReceivedEventArgs data)
        {
			// Detect if there is data received.
			if (string.IsNullOrEmpty(data.Data))
            {
                return;
            }

            Match match = postfixFailedAuthentication.Match(data.Data);
            
            // RegEx failed, likely not an authentication failure event
            if (!match.Success)
            {
                return;
            }

            string ip = match.Groups[1].Value;
            
            // Trigger login attempt event
            AccessAttempts accessAttempt = new AccessAttempts
            {
                IpAddress = ip,
                EventDate = DateTime.Now,
                Details = "SMTP Authentication failed.",
                AttackType = AttackType.BruteForce,
                Custom1 = "",
                Custom2 = 0,
                Custom3 = 0
            };

            // Log attempt
            log.Information($"{ip} failed SMTP authentication.");

            // Fire event
            accessAttemptsHandler?.Invoke(accessAttempt);
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
	}
}