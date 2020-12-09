using System;
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
                    .AddJsonFile("appSettings.json", false, true)
#endif
					.AddJsonFile("logSettings.json", false)
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
		/// Register the Block event handler
		/// </summary>
		/// <param name="blockHandler"></param>
		/// <returns></returns>
		public override async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
		{
			log.Debug($"Registered handler: {nameof(RegisterBlockHandler)}");

			BlockHandler = blockHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Register the Unblock event handler
		/// </summary>
		public override async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock unblockHandler)
		{
			log.Debug($"Registered handler: {nameof(RegisterUnblockHandler)}");

			UnblockHandler = unblockHandler;
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Block the IP in the firewall
		/// </summary>
		public override async Task<bool> BlockEvent(Blocks block)
		{
            // Ip Address to block
            string blockIp = blockIPRange
                ? block.IpAddressRange
                : block.IpAddress;

            // Create Rule Name
            block.FirewallRuleName = string.Format(nameTemplate, blockIp);

            // Create description
            string description = string.Format(descriptionTemplate, block.EventDate.ToString(dateFormat));

            // Create firewall rule
            //addRule.Name = block.FirewallRuleName;
            //addRule.Description = description;
            //addRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;

            //addRule.RemoteAddresses = blockIp;

            //addRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            //addRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            //addRule.Enabled = true;

            try
            {
                //fwPolicy2.Rules.Add(addRule);

                // Diagnostics
                log.Information(
                    $"Created firewall rules {block.FirewallRuleName} to block {blockIp}");
            }
            catch (Exception e)
            {
                // Diagnostics
                log.Error($"Unable to create firewall rule {block.FirewallRuleName}: {e.Message}");
            }

			return await Task.FromResult(true);
		}

		/// <summary>
		/// Unblock the IP in the firewall
		/// </summary>
		public override async Task<bool> UnblockEvent(Blocks block)
		{
			//INetFwPolicy2 firewallPolicy = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
			//firewallPolicy?.Rules.Remove(block.FirewallRuleName);

			// Diagnostics
			log.Information($"Removed firewall rule {block.FirewallRuleName} that blocked {block.IpAddress}");

			return await Task.FromResult(true);
		}
	}
}