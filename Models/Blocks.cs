using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Blocks")]
    public class Blocks
    {
        [Key] public long Id { get; set; }

        public string IpAddress { get; set; }
        public string Hostname { get; set; }

        public string Details { get; set; }
        public DateTime Date { get; set; }

        public string Country { get; set; }
        public string City { get; set; }
        public string ISP { get; set; }
    }
}