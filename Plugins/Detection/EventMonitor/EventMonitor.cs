using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Plugins;
using EventLogEntry = Plugins.Models.EventLogEntry;

namespace Plugins
{
    public class EventMonitor : IPluginBase
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

        public EventHandler LoginAttemptsHandler { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<bool> Initialize(PluginOptions options)
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
                    .ForContext(typeof(EventMonitor));

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
        public async Task<bool> Configure()
        {
            try
            {
                // Register EventLog
                EventLog eventLog = new EventLog("Security");
                eventLog.EntryWritten += EventLogOnEntryWritten;
                eventLog.EnableRaisingEvents = true;

                // Load actionable events from the configuration
                actionableEvents = config.GetSection("ActionableEvents").Get<List<long>>();

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
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterLoginAttemptHandler(EventHandler eventHandler)
        {
            LoginAttemptsHandler = eventHandler;
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterBlockHandler(EventHandler eventHandler)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        /// <param name="pluginEventArgs"></param>
        /// <returns></returns>
        public async Task<bool> LoginAttempt(PluginEventArgs pluginEventArgs)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        public async Task<bool> BlockedEvent(SP.Models.Blocks block)
        {
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
                // ignore
                return;
            }

            // Failed login
            if (e.Entry.InstanceId == (long) Events.FailedLogin)
            {
                // Parse entry into EventLogEntry
                EventLogEntry eventLogEntry = new EventLogEntry(e.Entry.ReplacementStrings);

                // Trigger login attempt event
                PluginEventArgs pluginEventArgs = new PluginEventArgs
                {
                    IPAddress = eventLogEntry.SourceNetworkAddress,
                    DateTime = DateTime.Now,
                    Details = $"Repeated RDP login failures. Last user: {eventLogEntry.AccountAccountName}"
                };

                // Log attempt
                log.Information(
                    $"Workstation {eventLogEntry.SourceWorkstationName} from {eventLogEntry.SourceNetworkAddress} failed logging in.");

                // Fire event
                LoginAttemptsHandler?.Invoke(this, pluginEventArgs);
            }
        }
    }
}