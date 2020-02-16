using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Core.Models
{
    [Table("Blocking")]
    public class Blocking
    {
        [Key] public long Id { get; set; }

        public string IpAddress { get; set; }
        public string Hostname { get; set; }
        public DateTime Date { get; set; }
    }
}