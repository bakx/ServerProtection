using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SP.Core.Interfaces;
using SP.Core.Plugin;
using SP.Core.Tools;
using SP.Models;
using SP.Plugins;

namespace SP.Core
{
	public class CoreService : BackgroundService
	{
		// Configuration object
		private readonly IConfigurationRoot config;

		// Caches entries of the IPData related objects (if enabled)
		private readonly MemoryCache ipDataCache = new MemoryCache("IpDataCache");

		// Caches entries of latest blocks
		private readonly MemoryCache latestBlocks = new MemoryCache("LatestBlocks");

		// Diagnostics
		private readonly ILogger<CoreService> log;

		// Contains all plugins that are loaded
		private readonly List<IPluginBase> plugins = new List<IPluginBase>();

		// Handlers
		private IApiHandler apiHandler;
		private readonly IProtectHandler protectHandler;

		// Configuration items
		private List<string> enabledPlugins;
		private bool blockIPRange;
		private bool ipDataEnabled;
		private string ipDataKey;
		private string ipDataUrl;
		private int unblockTimeSpanMinutes;

		/// <summary>
		/// </summary>
		/// <param name="log"></param>
		/// <param name="config"></param>
		/// <param name="protectHandler"></param>
		public CoreService(ILogger<CoreService> log, IConfigurationRoot config, IProtectHandler protectHandler)
		{
			this.log = log;
			this.config = config;
			this.protectHandler = protectHandler;

			// Login Attempts
			LoginAttemptEvent += OnLoginAttemptEvent;

			// Block events
			BlockEvent += OnBlockEvent;

			// Unblock events
			UnblockEvent += OnUnblockEvent;
		}

		// Unblock Timer
		public Timer UnblockTimer { get; private set; }

		/// <summary>
		/// </summary>
		protected async Task UnblockTask(object _)
		{
			List<Blocks> unblockList = await apiHandler.GetUnblock(unblockTimeSpanMinutes);

			if (unblockList != null)
			{
				if (unblockList.Any())
				{
					foreach (Blocks block in unblockList)
					{
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
		/// <param name="loginAttempt"></param>
		/// <returns></returns>
		private async void OnLoginAttemptEvent(LoginAttempts loginAttempt)
		{
			// Notify plug-ins of login attempt
			foreach (IPluginBase pluginBase in plugins)
			{
				await Task.Run(() => { pluginBase.LoginAttemptEvent(loginAttempt); });
			}

			// Default action for block
			bool block = false;

			// Attempt to contact the API 
			try
			{
				// Add the login attempt
				await protectHandler.AddLoginAttempt(loginAttempt);

				// Override?
				if (loginAttempt.OverrideBlock)
				{
					block = true;
				}
				else
				{
					// Analyze the login attempt to see if a block should be applied
					block = await protectHandler.AnalyzeAttempt(loginAttempt);
				}
			}
			catch (Exception ex)
			{
				log.LogError(ex.Message);
			}

			// Block IP?
			if (!block)
			{
				log.LogDebug($"{loginAttempt.IpAddress} will not be blocked.");
				return;
			}

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
				Details = loginAttempt.Details,
				AttackType = loginAttempt.AttackType
			};

			// Diagnostics
			log.LogDebug($"In routine to block {loginAttempt.IpAddress}");

			// If this IP address is found in the memory cache, it's likely that this IP address was used to 
			// bombard the server with login attempts and the plug-in architecture is catching up. During attacks
			// like that, it is possible that this method will be triggered multiple times.
			// Blocks will be cached for 5 minutes to prevent this.
			if (latestBlocks.Contains(blockIPRange ? loginAttempt.IpAddressRange : loginAttempt.IpAddress))
			{
				log.LogDebug($"{(blockIPRange ? loginAttempt.IpAddressRange : loginAttempt.IpAddress)} was recently blocked and this request will be ignored");
				return;
			}

			latestBlocks.Add(blockIPRange ? loginAttempt.IpAddressRange : loginAttempt.IpAddress, 1, DateTime.Now.AddMinutes(5));

			// Use IPData to get additional information about the IP
			if (ipDataEnabled)
			{
				DataModel dataModel = null;

				if (ipDataCache.Contains(loginAttempt.IpAddress))
				{
					dataModel = (DataModel) ipDataCache.GetCacheItem(loginAttempt.IpAddress)?.Value;
				}
				else
				{
					try
					{
						dataModel = await IPData.GetDetails(ipDataUrl, ipDataKey, loginAttempt.IpAddress);

						ipDataCache.Add(loginAttempt.IpAddress, dataModel, DateTime.Now.AddDays(7));
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

			// Notify plug-ins of block
			foreach (IPluginBase pluginBase in plugins)
			{
				await Task.Run(() => { pluginBase.BlockEvent(block); });
			}

			// Report block
			if (!await apiHandler.AddBlock(block))
			{
				log.LogError("An error occurred while reporting the block to the api.");
			}

			// Increase statistics
			await apiHandler.StatisticsUpdateBlocks(block);
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
			PluginOptions options = new PluginOptions()
			{
				BlockIPRange = blockIPRange
			};

			// Start plugins
			foreach (IPluginBase plugin in plugins)
			{
				// Initialize plugin
				await plugin.Initialize(options);

				// Initial configuration of plug
				await plugin.Configure();

				// Detect if this plug-in implements the Api handler
				if (plugin is IApiHandler handler)
				{
					apiHandler = handler;
					protectHandler.SetApiHandler(handler);
				}

				// Register handlers
				await plugin.RegisterLoginAttemptHandler(LoginAttemptEvent);
				await plugin.RegisterBlockHandler(BlockEvent);
				await plugin.RegisterUnblockHandler(UnblockEvent);
			}

			// Validate that an ApiHandler is active
			if (apiHandler == null)
			{
				log.LogError(
					"Unable to find an active ApiHandler plug-in. Please enable either the `api.https` or `api.grpc` plug-in.");
				return;
			}

			try
			{
				// Unblock timer
				UnblockTimer = new Timer(async state => await UnblockTask(state), null, TimeSpan.Zero,
					TimeSpan.FromMinutes(15));
			}
			catch(Exception e)
			{
				log.LogError(e.Message);
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

			// Should IP range (0/24) be blocked instead of single IP?
			blockIPRange = config.GetSection("Blocking:BlockIPRange").Get<bool>();

			// Get IPData configuration items
			ipDataUrl = config.GetSection("Tools:IPData:Url").Get<string>();
			ipDataKey = config.GetSection("Tools:IPData:Key").Get<string>();
			ipDataEnabled = config.GetSection("Tools:IPData:Enabled").Get<bool>();
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