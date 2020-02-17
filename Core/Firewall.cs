using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFwTypeLib;
using SP.Core.Interfaces;
using SP.Core.Models;

namespace SP.Core
{
    public class Firewall : IFirewall
    {
        private static readonly Type TypeFwPolicy2 =
            Type.GetTypeFromCLSID(new Guid("{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}"));

        private static readonly Type TypeFwRule =
            Type.GetTypeFromCLSID(new Guid("{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}"));

        private readonly string dateFormat;
        private readonly string descriptionTemplate;
        private readonly ILogger<Firewall> log; // Diagnostics

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
        }

        /// <summary>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="block"></param>
        public void Block(NET_FW_IP_PROTOCOL_ protocol, Blocks block)
        {
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
            INetFwRule addRule = (INetFwRule) Activator.CreateInstance(TypeFwRule);

            addRule.Profiles = fwPolicy2.CurrentProfileTypes;

            // Create Rule Name
            string ruleName = string.Format(nameTemplate, block.IpAddress);

            // Create description
            string description = string.Format(descriptionTemplate, block.Date.ToString(dateFormat));

            // Create firewall rule
            addRule.Name = ruleName;
            addRule.Description = description;
            addRule.Protocol = (int) protocol;
            addRule.RemoteAddresses = block.IpAddress;

            addRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            addRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            addRule.Enabled = true;

            fwPolicy2.Rules.Add(addRule);

            // Diagnostics
            log.LogInformation($"Created firewall rules {ruleName} to block {block.IpAddress}");
        }

        /// <summary>
        /// </summary>
        public void Unblock()
        {
            throw new NotImplementedException("WiP");

            //
            //
            //

            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);

            List<INetFwRule> list = fwPolicy2.Rules.Cast<INetFwRule>().ToList();
            INetFwRule firewallRule = list.SingleOrDefault(r => r.Name == ""); //eventData.RuleName);

            if (firewallRule != null)
            {
                // Diagnostics
                log.LogInformation($"Removed firewall rule {firewallRule.Name}");

                list.Remove(firewallRule);
            }
        }
    }
}