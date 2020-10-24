using SP.Models.Enums;

namespace SP.Overview.Helpers
{
	public static class AttackTypeString
	{
		public static string GetName(AttackType attackType)
		{
			return attackType switch
			{
				AttackType.WebSpam => "Web Spam",
				AttackType.BruteForce => "Brute-Force",
				AttackType.PortScan => "Port Scan",
				AttackType.SqlInjection => "Sql Injection",
				AttackType.WebExploit => "Web Exploit",
				_ => "Not specified"
			};
		}
	}
}