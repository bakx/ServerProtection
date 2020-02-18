using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Login.Attempts")]
    public class LoginAttempts
    {
        [Key] public long Id { get; set; }

        public string IpAddress { get; set; }
        public DateTime EventDate { get; set; }
        public string Details { get; set; }
    }
}