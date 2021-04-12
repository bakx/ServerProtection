using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
    public class IISMonitor : PluginBase
    {
        private Thread thread;
        private List<string> sitesMonitored;
        private List<string> exploitPaths;

        private IConfigurationRoot config;
        private ILogger log;
        private IPluginBase.AccessAttempt accessAttemptsHandler;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Task<bool> Initialize(PluginOptions options)
        {
            try
            {
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
                    .ForContext(typeof(IISMonitor));

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

                thread = new Thread(() =>
                {
                    using TraceEventSession session = new TraceEventSession("IIS-ETW");

                    // Enable IIS ETW provider and set up a new trace source on it
                    if (session.EnableProvider(IISLogTraceEventParser.ProviderName))
                    {
                        using ETWTraceEventSource traceSource =
                            new ETWTraceEventSource("IIS-ETW", TraceEventSourceType.Session);
                        IISLogTraceEventParser parser = new IISLogTraceEventParser(traceSource);

                        // Listen to event
                        parser.IISLog += ParserOnIISLog;

                        // Start processing
                        traceSource.Process();
                    }
                    else
                    {
                        log.Error($"Failed to enable provider ${IISLogTraceEventParser.ProviderName}");
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();

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
        private void ParserOnIISLog(IISLogTraceData data)
        {
            // Check if this site is monitored
            if (!sitesMonitored.Contains(data.ServiceName))
            {
                return;
            }

            // Check if path is in prohibited list
            bool isProhibited = exploitPaths.Any(p => data.UriStem.Contains(p));

            if (!isProhibited)
            {
                return;
            }

            // Trigger login attempt event
            AccessAttempts accessAttempt = new AccessAttempts
            {
                IpAddress = data.ClientIp,
                EventDate = DateTime.Now,
                Details = $"Attempt to access {data.UriStem} by IP {data.ClientIp}",
                OverrideBlock = true,
                AttackType = AttackType.WebExploit,
                Custom1 = data.UriStem,
                Custom2 = 0,
                Custom3 = 0
            };

            // Log attempt
            log.Information(
                $"Attempt to access {data.UriStem} on {data.ServiceName} by IP {data.ClientIp}.");

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