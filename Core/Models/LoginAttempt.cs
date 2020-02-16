using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Core.Models
{
    [Table("LoginAttempts")]
    public class LoginAttempt
    {
        [Key] public long Id { get; set; }

        public string IpAddress { get; set; }
        public DateTime EventDate { get; set; }
        public string Details { get; set; }
    }
}