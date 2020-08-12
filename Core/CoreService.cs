using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFwTypeLib;
using SP.Core.Interfaces;
using SP.Core.Plugin;
using SP.Core.Tools;
using SP.Models;
using SP.Plugins;

namespace SP.Core
{
    public class CoreService : BackgroundService
    {
        private readonly IApiHandler apiHandler;

        // Configuration object
        private readonly IConfigurationRoot config;
        private readonly IFirewall firewall;

        // Keep logs of last blocks that occurred during the following timespan Blocking.TimeSpanMinutes
        private readonly ConcurrentDictionary<string, DateTime> lastBlocks = new ConcurrentDictionary<string, DateTime>();

        // Keep cache of IPData requests (if enabled)
        private readonly ConcurrentDictionary<string, DataModel> ipdataCache = new ConcurrentDictionary<string, DataModel>();

        // Diagnostics
        private readonly ILogger<CoreService> log;

        // Contains all plugins that are loaded.
        private readonly List<IPluginBase> plugins = new List<IPluginBase>();

        // Handlers
        private readonly IProtectHandler protectHandler;

        // Configuration items
        private List<string> enabledPlugins;


        private bool ipDataEnabled;
        private string ipDataKey;
        private string ipDataUrl;
        private int unblockTimeSpanMinutes;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        /// <param name="firewall"></param>
        /// <param name="protectHandler"></param>
        /// <param name="apiHandler"></param>
        public CoreService(ILogger<CoreService> log, IConfigurationRoot config, IFirewall firewall,
            IProtectHandler protectHandler, IApiHandler apiHandler)
        {
            this.log = log;
            this.config = config;
            this.firewall = firewall;
            this.protectHandler = protectHandler;
            this.apiHandler = apiHandler;

            // Login Attempts
            LoginAttemptEvent += OnLoginAttemptEvent;

            // Block events
            BlockEvent += OnBlockEvent;

            // Unblock events
            UnblockEvent += OnUnblockEvent;

            /* This allows testing of the login attempt handler
            LoginAttempts attempt = new LoginAttempts();
            attempt.IpAddress = "192.130.131.132";
            attempt.EventDate = DateTime.Now;
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            LoginAttemptEvent.Invoke(attempt);
            */
        }

        // Unblock Timer
        public Timer UnblockTimer { get; private set; }

        // Recent Block Cache Timer
        public Timer RecentBlockCacheTimer { get; private set; }

        /// <summary>
        /// </summary>
        protected async Task UnblockTask(object state)
        {
            List<Blocks> unblockList = await apiHandler.GetUnblock(unblockTimeSpanMinutes);

            if (unblockList != null)
            {
                if (unblockList.Any())
                {
                    foreach (Blocks block in unblockList)
                    {
                        firewall.Unblock(block);
                        block.IsBlocked = 0;

                        // Notify listeners of unblock
                        UnblockEvent?.Invoke(block);

                        // Update block
                        await apiHandler.UpdateBlock(block);
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        protected async Task CleanCacheTask(object state)
        {
	        await Task.Run(() =>
	        {
		        List<KeyValuePair<string, DateTime>> keys = lastBlocks
			        .Where(l => l.Value.Ticks < DateTime.Now.Subtract(new TimeSpan(0, unblockTimeSpanMinutes, 0)).Ticks)
			        .ToList();

		        foreach ((string key, DateTime value) in keys)
		        {
			        log.LogDebug($"Clearing {key} with expiry date of {value} from {nameof(lastBlocks)} cache.");

                    lastBlocks.TryRemove(key, out _);
		        }
	        });
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <returns></returns>
        private async void OnLoginAttemptEvent(LoginAttempts loginAttempt)
        {
            // Notify plug-ins of login attempt
            foreach (IPluginBase pluginBase in plugins)
            {
                await Task.Run(() => { pluginBase.LoginAttemptEvent(loginAttempt); });
            }

            // Add the login attempt
            await protectHandler.AddLoginAttempt(loginAttempt);

            // Analyze the login attempt to see if a block should be applied
            bool block = await protectHandler.AnalyzeAttempt(loginAttempt);

            // Block IP?
            if (!block)
            {
                log.LogDebug($"{loginAttempt.IpAddress} will not be blocked.");
                return;
            }

            // In some cases, it's possible due a massive attack there are multiple events fired at the same time.
            // This part attempts to prevent duplicate firewall rules.
            if (lastBlocks.ContainsKey(loginAttempt.IpAddressRange))
            {
	            log.LogDebug($"{loginAttempt.IpAddress} has range match {loginAttempt.IpAddressRange} in cache and should already have been blocked");
	            return;
            }

            // Add IP to list of last 3 blocks
            lastBlocks.TryAdd(loginAttempt.IpAddressRange, DateTime.Now);

            // Signal block
            BlockEvent?.Invoke(loginAttempt);
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        private async void OnBlockEvent(LoginAttempts loginAttempt)
        {
            // Create EventData object
            Blocks block = new Blocks
            {
                IpAddress = loginAttempt.IpAddress,
                Hostname = "",
                Date = loginAttempt.EventDate,
                Details = loginAttempt.Details
            };

            // Use IPData to get additional information about the IP
            if (ipDataEnabled)
            {
	            DataModel dataModel = null;

                if (ipdataCache.ContainsKey(loginAttempt.IpAddress))
	            {
		            dataModel = ipdataCache[loginAttempt.IpAddress];
                }
	            else
	            {
		            try
		            {
			            dataModel = await IPData.GetDetails(ipDataUrl, ipDataKey, loginAttempt.IpAddress);
			            ipdataCache.TryAdd(loginAttempt.IpAddress, dataModel);

		            }
		            catch (Exception ex)
		            {
			            log.LogError(
				            $"Unable to get additional details about ip {block.IpAddress}. Service response: {ex.Message}");
		            }
	            }

                // Only on successful retrieval of dataModel populate the block object
                if (dataModel != null)
                {
	                block.Country = dataModel.Country;
	                block.City = dataModel.City;
	                block.ISP = dataModel.ISP;
                }
            }

            // Attempt to get the hostname
            try
            {
                IPHostEntry hostInfo = await Dns.GetHostEntryAsync(block.IpAddress);
                block.Hostname = hostInfo.HostName;
            }
            catch
            {
                log.LogDebug($"Unable to get host name for {block.IpAddress}");
            }

            // Increase statistics
            await apiHandler.StatisticsUpdateBlocks(block);

            // Block IP in Firewall
            firewall.Block(NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY, block);

            // Report block
            if (!await apiHandler.AddBlock(block))
            {
                log.LogInformation("An error occured while reporting the block to the api.");
            }

            // Notify plug-ins of block
            foreach (IPluginBase pluginBase in plugins)
            {
                await Task.Run(() => { pluginBase.BlockEvent(block); });
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="block"></param>
        private async void OnUnblockEvent(Blocks block)
        {
            // Notify plug-ins of unblock
            foreach (IPluginBase pluginBase in plugins)
            {
                await Task.Run(() => { pluginBase.UnblockEvent(block); });
            }

            // Do not proceed if this IP does not exists in the recently block list
            if (!lastBlocks.ContainsKey(block.IpAddress))
            {
                return;
            }

            // If this IP exists in the recently block list, remove it.
            log.LogDebug($"Removing {block.IpAddress} from the last block list");
            lastBlocks.TryRemove(block.IpAddress, out _);
        }

        // Events
        public event IPluginBase.LoginAttempt LoginAttemptEvent;
        public event IPluginBase.Block BlockEvent;
        public event IPluginBase.Unblock UnblockEvent;

        /// <summary>
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure()
            Configure();

            // Load plugins
            LoadPlugins();

            // Plugin options
            PluginOptions options = new PluginOptions();

            // Start plugins
            foreach (IPluginBase plugin in plugins)
            {
                // Initialize plugin
                await plugin.Initialize(options);

                // Initial configuration of plug
                await plugin.Configure();

                // Register handlers
                await plugin.RegisterLoginAttemptHandler(LoginAttemptEvent);
                await plugin.RegisterBlockHandler(BlockEvent);
                await plugin.RegisterUnblockHandler(UnblockEvent);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(-1, stoppingToken);
            }
        }

        /// <summary>
        /// </summary>
        private void Configure()
        {
            // List of plug-ins that should be enabled
            enabledPlugins = config.GetSection("Plugins").Get<List<string>>();

            // Retrieve the unblock timespan minutes
            unblockTimeSpanMinutes = config.GetSection("Blocking:UnblockTimeSpanMinutes").Get<int>();

            // Get IPData configuration items
            ipDataUrl = config.GetSection("Tools:IPData:Url").Get<string>();
            ipDataKey = config.GetSection("Tools:IPData:Key").Get<string>();
            ipDataEnabled = config.GetSection("Tools:IPData:Enabled").Get<bool>();

            // Unblock timer
            UnblockTimer = new Timer(async state => await UnblockTask(state), null, TimeSpan.Zero,
                TimeSpan.FromMinutes(15));

            // Clear last block cache older than 15 minutes
            RecentBlockCacheTimer = new Timer(async state => await CleanCacheTask(state), null, TimeSpan.Zero,
	            TimeSpan.FromMinutes(15));
        }

        /// <summary>
        /// </summary>
        private void LoadPlugins()
        {
            Plugins<IPluginBase> pluginLoader = new Plugins<IPluginBase>(log);

            // Paths that contain plugins.
            string[] pluginPaths =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "plugins")
            };

            // Loop through all plugin paths and retrieve all plugins that match the naming requirements.
            foreach (string pluginPath in pluginPaths)
            {
                // Diagnostics.
                log.LogInformation("Browsing folder {0} for plugins", pluginPath);

                // Read all subdirectories of the plugin folder being processed.
                foreach (DirectoryInfo directoryInfo in new DirectoryInfo(pluginPath).GetDirectories())
                {
                    // From all subdirectories, retrieve all plugins that are compliant with the naming standards plugins.<name>.dll
                    IEnumerable<FileInfo> plugin = directoryInfo
                        .GetFiles("*.dll")
                        .Where(d =>
                            d.Name.ToLowerInvariant().StartsWith("plugins.") &&
                            !d.Name.ToLowerInvariant().StartsWith("plugins.base"))
                        .ToList();

                    // Loop through all plugins found in the directory.
                    foreach (FileInfo fileInfo in plugin)
                    {
                        int prefixLength = "plugins.".Length;
                        int suffixLength = fileInfo.Extension.Length;

                        // Get the name of the plugin.
                        string pluginName =
                            fileInfo.Name
                                .Remove(0, prefixLength)
                                .Remove(fileInfo.Name.Length - prefixLength - suffixLength, suffixLength)
                                .ToLowerInvariant();

                        if (!enabledPlugins.Contains(pluginName))
                        {
                            log.LogInformation($"Plugin {pluginName} is not enabled");
                            continue;
                        }

                        // One or more command line arguments mention the plugin name.
                        IEnumerable<IPluginBase> currentPlugin = plugin
                            .Select(file => pluginLoader.LoadPlugin(file.FullName))
                            .Select(pluginLoader.CreateCommands)
                            .SelectMany(d => d);

                        // Add all plugin matches to the main holder.
                        plugins.AddRange(currentPlugin);
                    }
                }
            }
        }
    }
}