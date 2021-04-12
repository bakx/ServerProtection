using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
    public class Firewall : PluginBase
    {
        // Diagnostics
        private ILogger log;

        //
        // sudo yum install iptables-services
        // sudo systemctl start iptables
        // sudo systemctl enable iptables
        //

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Task<bool> Initialize(PluginOptions options)
        {
            try
            {
                // Initiate the configuration
                IConfigurationRoot config = new ConfigurationBuilder()
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
                    .ForContext(typeof(Firewall));

                // Diagnostics
                log.Information("Plugin initialized");

                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                if (log == null)
                {
                    Console.WriteLine(e);
                    File.WriteAllText(nameof(Firewall), e.Message);
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
        /// Block the IP in the firewall
        /// </summary>
        public override async Task<bool> BlockEvent(Blocks block)
        {
            try
            {
                block.FirewallRuleName = "";

                await ExecuteEvent(ActionType.Add, block);
                await Save();

                // Diagnostics
                log.Information($"Created firewall rule that blocks {block.IpAddress}");

                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                log.Error($"Unable to create firewall rule : {e.Message}");
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Unblock the IP in the firewall
        /// </summary>
        public override async Task<bool> UnblockEvent(Blocks block)
        {
            try
            {
                block.FirewallRuleName = "";

                await ExecuteEvent(ActionType.Remove, block);
                await Save();

                // Diagnostics
                log.Information($"Removed firewall rule that blocked {block.IpAddress}");

                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                log.Error($"Unable to remove firewall rule : {e.Message}");
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        private async Task ExecuteEvent(ActionType type, Blocks block)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = "sudo",
                        Arguments =
                            $"iptables -{(type == ActionType.Add ? "I" : "D")} INPUT -s {block.IpAddress} -j DROP"
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
            }
            catch (Exception e)
            {
                // Diagnostics
                log.Error(
                    $"Unable to {(type == ActionType.Add ? "create" : "remove")} firewall rule {block.IpAddress}: {e.Message}");
            }
        }

        private enum ActionType
        {
            Add,
            Remove
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Save()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = "sudo",
                        Arguments = "service iptables save"
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                // Diagnostics
                log.Information("Saved firewall rule");
            }
            catch (Exception e)
            {
                // Diagnostics
                log.Error($"Unable to save firewall rule : {e.Message}");
            }
        }
    }
}