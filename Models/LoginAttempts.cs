using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Login.Attempts")]
    public class LoginAttempts
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

        public DateTime EventDate { get; set; }
        public string Details { get; set; }
    }
}