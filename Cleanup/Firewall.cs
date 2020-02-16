using System;
using System.Collections.Generic;
using System.Linq;
using NetFwTypeLib;

namespace SP.Core
{
    public static class Firewall
    {
        private static readonly Type TypeFwPolicy2 =
            Type.GetTypeFromCLSID(new Guid("{E2B3C97F-6AE1-41AC-817A-F6F92166D7DD}"));

        /// <summary>
        /// </summary>
        public static void Clear()
        {
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2) Activator.CreateInstance(TypeFwPolicy2);

            List<INetFwRule> list = fwPolicy2.Rules.Cast<INetFwRule>().ToList();

            foreach (INetFwRule rule in list.Where(r => r.Name.ToLowerInvariant().StartsWith("rdp attack")))
            {
                Console.WriteLine($"Deleting {rule.Name}");
                fwPolicy2.Rules.Remove(rule.Name);
            }

            
        }
    }
}