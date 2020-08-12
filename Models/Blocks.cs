using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Blocks")]
    public class Blocks
    {
        private string ipAddress;

        [Key] public long Id { get; set; }

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
        public string IpAddressRange => $"{IpAddress1}.{IpAddress2}.{IpAddress3}.0";

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
        /// Hostname of Ip when attempt to place
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Country of Ip when attempt to place
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// City of Ip when attempt to place
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ISP of Ip when attempt to place
        /// </summary>
        public string ISP { get; set; }

        public string Details { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// The name of the rule in the firewall
        /// </summary>
        public string FirewallRuleName { get; set; }

        /// <summary>
        /// Used in conjunction to remove the firewall rules
        /// </summary>
        public byte IsBlocked { get; set; } = 1;
    }
}