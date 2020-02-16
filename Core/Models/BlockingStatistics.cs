using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Core.Models
{
    [Table("BlockingStatistics")]
    public class BlockingStatistics
    {
        [Key] public long Id { get; set; }

        public string Country { get; set; }
        public long Attempts { get; set; }
    }
}