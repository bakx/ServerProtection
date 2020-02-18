using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Plugins;

namespace Testing
{
    public class TestSignalR
    {
        private string loginAttemptsHubUrl;
        private string blocksHubUrl;

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
                    .ForContext(typeof(TestSignalR));

                // Assign config variables
                loginAttemptsHubUrl = config["loginAttemptsHubUrl"];
                blocksHubUrl = config["blocksHubUrl"];

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
                // Login attempts hub
  
                HubLoginAttempts = new HubConnectionBuilder()
                    .WithUrl(loginAttemptsHubUrl)
                    //                    .AddMessagePackProtocol()
                    .Build();

                HubLoginAttempts.Closed += async error =>
                {
                    log.Warning("Disconnected. Reconnecting...");

                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await HubLoginAttempts.StartAsync();
                };

                await HubLoginAttempts.StartAsync();

                // Blocks hub

                HubBlocks = new HubConnectionBuilder()
                    .WithUrl(blocksHubUrl)
                    //                    .AddMessagePackProtocol()
                    .Build();

                HubBlocks.Closed += async error =>
                {
                    log.Warning("Disconnected. Reconnecting...");

                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await HubBlocks.StartAsync();
                };

                await HubBlocks.StartAsync();

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

        public HubConnection HubLoginAttempts { get; set; }
        public HubConnection HubBlocks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginEventArgs"></param>
        /// <returns></returns>
        public async Task<bool> LoginAttempt(PluginEventArgs pluginEventArgs)
        {
            try
            {
                await HubLoginAttempts.InvokeAsync("LoginAttempt", pluginEventArgs);
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
            try
            {
                await HubBlocks.InvokeAsync("Block", pluginEventArgs);
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return false;
            }
        }
    }
}