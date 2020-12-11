using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Plugins.Models;
using Serilog;
using SP.Models;
using SP.Models.Enums;
using SP.Plugins;

namespace Plugins
{
    public class Honeypot : PluginBase
    {
        private Thread thread;
        private List<int> portsMonitored;

        private IConfigurationRoot config;
        private ILogger log;
        private IPluginBase.AccessAttempt accessAttemptsHandler;

        public event EventHandler<ConnectionEventArgs> ConnectionRequest;

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
                    .AddJsonFile("appSettings.development.json", false)
#else
                    .AddJsonFile("appSettings.json", false)
#endif
                    .AddJsonFile("logSettings.json", false)
                    .Build();

                log = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .CreateLogger()
                    .ForContext(typeof(Honeypot));

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
                // Load ports monitored from the configuration
                portsMonitored = config.GetSection("PortsMonitored").Get<List<int>>();

                ConnectionRequest += OnConnectionRequest;

                foreach (int port in portsMonitored)
                {
                    thread = new Thread(() =>
                    {
                        TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
                        tcpListener.Start();

                        // Diagnostics
                        log.Information($"Listening on port {port}");

                        // Enter the listening loop.
                        while (true)
                        {
                            // Perform a blocking call to accept requests.
                            TcpClient client = tcpListener.AcceptTcpClient();

                            // Update Events.
                            ConnectionRequest?.Invoke(this, new ConnectionEventArgs(client));

                            // Remove client
                            client.Dispose();
                        }

                        // ReSharper disable once FunctionNeverReturns
                    })
                    {
                        IsBackground = true
                    };

                    thread.Start();
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

        private void OnConnectionRequest(object sender, ConnectionEventArgs e)
        {
            // Trigger login attempt event
            AccessAttempts accessAttempt = new AccessAttempts
            {
                IpAddress = e.IpAddress,
                EventDate = DateTime.Now,
                Details = $"Attempt to access port {e.Port} by IP {e.IpAddress}",
                OverrideBlock = true,
                AttackType = AttackType.PortScan,
                Custom1 = "",
                Custom2 = e.Port,
                Custom3 = 0
            };

            // Log attempt
            log.Information($"Attempt to access port {e.Port} by IP {e.IpAddress}");

            // Fire event
            accessAttemptsHandler?.Invoke(accessAttempt);
        }

        /// <summary>
        /// Register the AccessAttemptHandler in order to fire events
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