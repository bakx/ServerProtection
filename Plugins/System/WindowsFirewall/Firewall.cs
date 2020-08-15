using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NetFwTypeLib;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class Firewall : IPluginBase
	{
		private static readonly Type TypeFwPolicy2 =
			Type.GetTypeFromCLSID(new Guid("{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}"));

		private static readonly Type TypeFwRule =
			Type.GetTypeFromCLSID(new Guid("{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}"));

		private bool blockIPRange;
		private string dateFormat;
		private string descriptionTemplate;

		// Diagnostics
		private ILogger log;

		// Configuration options
		private string nameTemplate;

		// Handlers
		public IPluginBase.Block BlockHandler { get; set; }
		public IPluginBase.Unblock UnblockHandler { get; set; }

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
					.ForContext(typeof(Firewall));

				// Configuration settings
				nameTemplate = config.GetSection("Firewall:Rules:NameTemplate").Get<string>();
				descriptionTemplate = config.GetSection("Firewall:Rules:DescriptionTemplate").Get<string>();
				dateFormat = config.GetSection("Firewall:Rules::DateFormat").Get<string>();
				blockIPRange = config.GetSection("Blocking:BlockIPRange").Get<bool>();

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
		/// Not used by this plugin
		/// </summary>
		/// <param name="loginAttemptHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterLoginAttemptHandler(IPluginBase.LoginAttempt loginAttemptHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Register the Block event handler
		/// </summary>
		/// <param name="blockHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
		{
			BlockHandler = blockHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Register the Unblock event handler
		/// </summary>
		public async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock unblockHandler)
		{
			UnblockHandler = unblockHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Report the ip to AbuseIP
		/// </summary>
		public async Task<bool> BlockEvent(Blocks block)
		{
			INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
			INetFwRule addRule = (INetFwRule) Activator.CreateInstance(TypeFwRule);

			if (addRule != null && fwPolicy2 != null)
			{
				addRule.Profiles = fwPolicy2.CurrentProfileTypes;

				// Ip Address to block
				string blockIp = blockIPRange
					? block.IpAddressRange
					: block.IpAddress;

				// Create Rule Name
				block.FirewallRuleName = string.Format(nameTemplate, blockIp);

				// Create description
				string description = string.Format(descriptionTemplate, block.Date.ToString(dateFormat));

				// Create firewall rule
				addRule.Name = block.FirewallRuleName;
				addRule.Description = description;
				addRule.Protocol = (int) NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;

				addRule.RemoteAddresses = blockIp;

				addRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
				addRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
				addRule.Enabled = true;

				try
				{
					fwPolicy2.Rules.Add(addRule);

					// Diagnostics
					log.Information(
						$"Created firewall rules {block.FirewallRuleName} to block {blockIp}");
				}
				catch (Exception e)
				{
					// Diagnostics
					log.Error($"Unable to create firewall rule {block.FirewallRuleName}: {e.Message}");
				}
			}
			else
			{
				// Diagnostics
				log.Error(
					$"Unable to add firewall rules. Null values: addRule = {addRule == null}, fwPolicy2 = {fwPolicy2 == null}");
			}

			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> UnblockEvent(Blocks block)
		{
			INetFwPolicy2 firewallPolicy = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
			firewallPolicy?.Rules.Remove(block.FirewallRuleName);

			// Diagnostics
			log.Information($"Removed firewall rule {block.FirewallRuleName} that blocked {block.IpAddress}");

			return await Task.FromResult(true);
		}
	}
}