using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFwTypeLib;
using SP.Core.Interfaces;
using SP.Models;

namespace SP.Core
{
    public class Firewall : IFirewall
    {
        private static readonly Type TypeFwPolicy2 =
            Type.GetTypeFromCLSID(new Guid("{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}"));

        private static readonly Type TypeFwRule =
            Type.GetTypeFromCLSID(new Guid("{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}"));

        private readonly bool blockIPRange;
        private readonly string dateFormat;
        private readonly string descriptionTemplate;

        //
        private readonly ILogger<Firewall> log;

        // Configuration settings
        private readonly string nameTemplate;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        public Firewall(ILogger<Firewall> log, IConfigurationRoot config)
        {
            this.log = log;

            // Configuration settings
            nameTemplate = config.GetSection("Firewall:Rules:NameTemplate").Get<string>();
            descriptionTemplate = config.GetSection("Firewall:Rules:DescriptionTemplate").Get<string>();
            dateFormat = config.GetSection("Firewall:Rules::DateFormat").Get<string>();
            blockIPRange = config.GetSection("Blocking:BlockIPRange").Get<bool>();
        }

        /// <summary>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="block"></param>
        public void Block(NET_FW_IP_PROTOCOL_ protocol, Blocks block)
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
	            addRule.Protocol = (int) protocol;

	            addRule.RemoteAddresses = blockIp;

	            addRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
	            addRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
	            addRule.Enabled = true;

	            try
	            {
		            fwPolicy2.Rules.Add(addRule);

		            // Diagnostics
		            log.LogInformation(
			            $"Created firewall rules {block.FirewallRuleName} to block {blockIp}");
	            }
	            catch (Exception e)
	            {
		            // Diagnostics
		            log.LogError($"Unable to create firewall rule {block.FirewallRuleName}: {e.Message}");
                }
            }
            else
            {
	            // Diagnostics
	            log.LogError($"Unable to add firewall rules. Null values: addRule = {addRule == null}, fwPolicy2 = {fwPolicy2 == null}");
            }
        }

        /// <summary>
        /// </summary>
        public void Unblock(Blocks block)
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
            firewallPolicy?.Rules.Remove(block.FirewallRuleName);

            // Diagnostics
            log.LogInformation($"Removed firewall rule {block.FirewallRuleName} that blocked {block.IpAddress}");
        }
    }
}