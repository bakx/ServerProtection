using System;
using System.Collections.Generic;
using System.Linq;
using NetFwTypeLib;
using Serilog;

namespace SP.Core
{
    public static class Firewall
    {
        // Logging
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(Firewall));

        private static readonly Type TypeFwPolicy2 =
            Type.GetTypeFromCLSID(new Guid("{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}"));

        private static readonly Type TypeFwRule =
            Type.GetTypeFromCLSID(new Guid("{2C5BC43E-3369-4C33-AB0C-BE9469677AF4}"));

        public static void Block(NET_FW_IP_PROTOCOL_ protocol, EventData eventData)
        {
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);
            INetFwRule addRule = (INetFwRule) Activator.CreateInstance(TypeFwRule);

            addRule.Profiles = fwPolicy2.CurrentProfileTypes;

            addRule.Name = eventData.RuleName;
            addRule.Description = eventData.Description;
            addRule.Protocol = (int) protocol;
            addRule.RemoteAddresses = eventData.Address;

            if (eventData.Port != "")
            {
                addRule.LocalPorts = eventData.Port;
            }

            addRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            addRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            addRule.Enabled = true;

            fwPolicy2.Rules.Add(addRule);

            // Diagnostics
            Log.Information($"Created firewall rules {eventData.RuleName} to block {eventData.Address}");
        }

        public static void Unblock(NET_FW_IP_PROTOCOL_ protocol, EventData eventData)
        {
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);

            List<INetFwRule> list = fwPolicy2.Rules.Cast<INetFwRule>().ToList();
            INetFwRule a = list.SingleOrDefault(r => r.Name == eventData.RuleName);

            if (a != null)
            {
                // Diagnostics
                Log.Information($"Removed firewall rule {eventData.RuleName}");
            }
        }
    }

    public class EventData
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
    }
}