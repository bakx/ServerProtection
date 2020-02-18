using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Plugins;

namespace Plugins
{
    public class LiveReportSignalR : IPluginBase
    {
        private string loginAttemptsHubUrl;

        // Diagnostics
        private ILogger log;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<bool> Initialize(PluginOptions options)
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
                loginAttemptsHubUrl = config["loginAttemptsHubUrl"];

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
        public async Task<bool> Configure()
        {
            try
            {
                Hub = new HubConnectionBuilder()
                    .WithUrl(loginAttemptsHubUrl)
//                    .AddMessagePackProtocol()
                    .Build();

                Hub.Closed += async error =>
                {
                    log.Warning("Disconnected. Reconnecting...");

                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await Hub.StartAsync();
                };

                await Hub.StartAsync();
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

        public HubConnection Hub { get; set; }

        /// <summary>
        /// Not used by this plug-in
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterLoginAttemptHandler(EventHandler eventHandler)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plug-in
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterBlockHandler(EventHandler eventHandler)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginEventArgs"></param>
        /// <returns></returns>
        public async Task<bool> LoginAttempt(PluginEventArgs pluginEventArgs)
        {
            try
            {
                await Hub.InvokeAsync("LoginAttempt", pluginEventArgs);
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return false;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task<bool> BlockedEvent(PluginEventArgs pluginEventArgs)
        {
            return await Task.FromResult(true);
        }
    }
}