using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Statistics.Blocks")]
    public class StatisticsBlocks
    {
        [Key] public long Id { get; set; }

        public string Country { get; set; }
        public string City { get; set; }
        public string ISP { get; set; }
        public long Attempts { get; set; }
    }
}