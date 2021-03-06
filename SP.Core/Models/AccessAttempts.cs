﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SP.Models.Enums;

namespace SP.Models
{
	[Table("Access.Attempts")]
	public class AccessAttempts
	{
		private string ipAddress;

		/// <summary>
		/// Unique identifier for the attempt.
		/// </summary>
		[Key]
		public long Id { get; set; }

		/// <summary>
		/// Identifies the source of the attempt.
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// IP Address of machine that triggered the event.
		/// </summary>
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
		/// Returns a 0/24 presentation of the IP address (e.g., 192.168.1.0)
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
		/// The date the event took place.
		/// </summary>
		public DateTime EventDate { get; set; }

		/// <summary>
		/// Details about the attempt. E.g., RDP brute force with username administrator.
		/// </summary>
		public string Details { get; set; }

		/// <summary>
		/// This property can be set from plug-ins to identify the type of Attack.
		/// </summary>
		public AttackType AttackType { get; set; }

		/// <summary>
		/// Custom field that can be set to anything depending on the plug-in.
		/// </summary>
		public string Custom1 { get; set; }

		/// <summary>
		/// Custom field that can be set to anything depending on the plug-in.
		/// </summary>
		public int Custom2 { get; set; }

		/// <summary>
		/// Custom field that can be set to anything depending on the plug-in.
		/// </summary>
		public long Custom3 { get; set; }

		/// <summary>
		/// This property can be set from plug-ins to overwrite the block mechanism.
		/// Login attempts in general can be attempted N times, while bots searching for exploitable
		/// paths should be blocked right away.
		/// </summary>
		[NotMapped]
		public bool OverrideBlock { get; set; }
	}
}