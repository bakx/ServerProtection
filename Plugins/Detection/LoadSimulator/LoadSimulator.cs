using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
    public class LoadSimulator : IPluginBase
    {
        //
        private int parallelThreads;
        private Timer timer;

        private IConfigurationRoot config;
        private ILogger log;
        private IPluginBase.LoginAttempt loginAttemptsHandler;

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
                    .ForContext(typeof(LoadSimulator));

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
                // Load configuration
                parallelThreads = config.GetSection("ParallelThreads").Get<int>();

                timer = new Timer(Callback, null, TimeSpan.FromSeconds(20), TimeSpan.Zero);

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
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void Callback(object state)
        {
            // Disable timer
            timer.Change(0, 5);

            Parallel.For((long)1, parallelThreads, (i, res) =>
            {
                // Trigger login attempt event
                LoginAttempts loginAttempt = new LoginAttempts
                {
                    IpAddress = $"123.123.123.{i}",
                    EventDate = DateTime.Now,
                    Details = "Repeated RDP login failures. Last user: loadTest"
                };

                // Log attempt
                log.Information(
                    "Load test simulation.");

                // Fire event
                loginAttemptsHandler?.Invoke(loginAttempt);
            });
        }

        /// <summary>
        /// Register the LoginAttemptsHandler in order to fire events
        /// </summary>
        /// <param name="loginAttemptHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterLoginAttemptHandler(IPluginBase.LoginAttempt loginAttemptHandler)
        {
            loginAttemptsHandler = loginAttemptHandler;
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        /// <param name="blockHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        /// <param name="unblockHandler"></param>
        /// <returns></returns>
        public async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock unblockHandler)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <returns></returns>
        public async Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        public async Task<bool> BlockEvent(Blocks block)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Not used by this plugin
        /// </summary>
        public async Task<bool> UnblockEvent(Blocks block)
        {
            return await Task.FromResult(true);
        }
    }
}