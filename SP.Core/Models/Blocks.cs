using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SP.Models.Enums;

namespace SP.Models
{
	[Table("Blocks")]
	public class Blocks
	{
		private string ipAddress;

		/// <summary>
		/// Unique identifier for the block.
		/// </summary>
		[Key]
		public long Id { get; set; }

		public string IpAddress
		{
			get => ipAddress;
			set
			{
				string[] ip = value.Split(".");
				IpAddress1 = Convert.ToByte(ip[0]);
				IpAddress2 = Convert.ToByte(ip[1]);
				IpAddress3 = Convert.ToByte(ip[2]);
				IpAddress4 = Convert.ToByte(ip[3]);

				ipAddress = value;
			}
		}

		/// <summary>
		/// Returns a 0/24 presentation of the IP address (e.g., 192.168.1.0/24)
		/// </summary>
		public string IpAddressRange => $"{IpAddress1}.{IpAddress2}.{IpAddress3}.0/24";

		/// <summary>
		/// Used to split up performance for IP range scanning.
		/// </summary>
		public byte IpAddress1 { get; set; }

		/// <summary>
		/// Used to split up performance for IP range scanning.
		/// </summary>
		public byte IpAddress2 { get; set; }

		/// <summary>
		/// Used to split up performance for IP range scanning.
		/// </summary>
		public byte IpAddress3 { get; set; }

		/// <summary>
		/// Used to split up performance for IP range scanning.
		/// </summary>
		public byte IpAddress4 { get; set; }

		/// <summary>
		/// Hostname of IP that is being blocked.
		/// </summary>
		public string Hostname { get; set; }

		/// <summary>
		/// Country of the IP that is being blocked.
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// City of IP that is being blocked.
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// ISP of IP that is being blocked.
		/// </summary>
		public string ISP { get; set; }

		/// <summary>
		/// Details about the block. E.g., RDP brute force with username administrator.
		/// </summary>
		public string Details { get; set; }

		/// <summary>
		/// Date when the block was activated.
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// Name of the rule in the firewall.
		/// </summary>
		public string FirewallRuleName { get; set; }

		/// <summary>
		/// Indicates if an IP is blocked. Used in conjunction to remove the firewall rules.
		/// </summary>
		public byte IsBlocked { get; set; } = 1;

		/// <summary>
		/// This property is used to identify the type of Attack and log it for statistics.
		/// </summary>
		public AttackType AttackType { get; set; }
	}
}