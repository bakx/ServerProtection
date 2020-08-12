using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            List<INetFwRule> list = fwPolicy2?.Rules.Cast<INetFwRule>()
                .Where(r => r.Name.ToLowerInvariant().StartsWith("sp service block")).ToList();

            if (list == null)
            {
	            Console.WriteLine("Unable to find any rules");

	            return;
            }

            Console.WriteLine($"Found {list.Count} rules that should be deleted.");

            using StreamWriter writer = new StreamWriter("remove.ps1");
            foreach (INetFwRule rule in list)
            {
	            Console.WriteLine($"Deleting {rule.Name}");
	            writer.WriteLine($"Remove-NetFirewallRule \"{rule.Name}\"");

	            Task.Factory.StartNew(() =>
	            {
		            try
		            {
			            fwPolicy2.Rules.Remove(rule.Name);
		            }
		            catch (Exception e)
		            {
			            Console.WriteLine(e.Message);
		            }
	            });
            }
        }
    }
}