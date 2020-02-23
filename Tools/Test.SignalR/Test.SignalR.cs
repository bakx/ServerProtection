using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Testing
{
    public class TestSignalR
    {
        // Diagnostics
        private ILogger log;
        private string reportingHubUrl;

        public HubConnection Hub { get; set; }

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
        public async Task<bool> Configure()
        {
            try
            {
                // Login attempts hub

                Hub = new HubConnectionBuilder()
                    .WithUrl(reportingHubUrl)
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

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <returns></returns>
        public async Task<bool> LoginAttempt(LoginAttempts loginAttempt)
        {
            try
            {
                await Hub.InvokeAsync("LoginAttempt", loginAttempt);
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// </summary>
        public async Task<bool> BlockedEvent(Blocks block)
        {
            try
            {
                await Hub.InvokeAsync("Block", block);
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// </summary>
        public async Task<bool> UnblockedEvent(Blocks block)
        {
            try
            {
                await Hub.InvokeAsync("Unblock", block);
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