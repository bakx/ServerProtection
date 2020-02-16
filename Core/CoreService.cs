using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SP.Plugins;

namespace SP.Core
{
    public class CoreService : BackgroundService
    {
        // Configuration object
        private readonly IConfigurationRoot config;

        // Diagnostics
        private readonly ILogger<CoreService> log;

        // Contains all plugins that are loaded.
        private readonly List<IPluginBase> plugins = new List<IPluginBase>();

        // Configuration items
        private List<string> configPlugins;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        public CoreService(ILogger<CoreService> log, IConfigurationRoot config)
        {
            this.log = log;
            this.config = config;

            // Handle events
            //

            // Login Attempts
            LoginAttemptEvent += (sender, args) =>
            {
                PluginEventArgs pluginEventArgs = args as PluginEventArgs;

                // This should call logic that figures out the login attempts over last
                // x minutes and then potentially ban the ip.

                // Logic determines it should be banned.
                if (1 == 0)
                {
                    BlockEvent?.Invoke(this, args);
                }
            };

            // Block events
            BlockEvent += async (sender, args) =>
            {
                PluginEventArgs pluginEventArgs = args as PluginEventArgs;

                try
                {
                    foreach (IPluginBase pluginBase in plugins)
                    {
                        await Task.Run(() => { pluginBase.BlockedEvent(pluginEventArgs); });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            };
        }

        // Events
        public event EventHandler LoginAttemptEvent;
        public event EventHandler BlockEvent;

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
            configPlugins = config.GetSection("Plugins").Get<List<string>>();
        }

        /// <summary>
        /// </summary>
        private void LoadPlugins()
        {
            Plugins<IPluginBase> pluginLoader = new Plugins<IPluginBase>(log);

            // Paths that contain plugins.
            string[] pluginPaths =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins")
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

                        if (!configPlugins.Contains(pluginName))
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