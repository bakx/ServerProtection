using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP.Models
{
    [Table("Statistics.Blocks")]
    public class StatisticsBlocks
    {
	    /// <summary>
	    /// Unique identifier for the statistic.
	    /// </summary>
        [Key] public long Id { get; set; }

        /// <summary>
        /// Country of the IP that is being blocked.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// City of the IP that is being blocked.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ISP of the IP that is being blocked.
        /// </summary>
        public string ISP { get; set; }

        /// <summary>
        /// Amount of login attempts made by the IP that is being blocked.
        /// </summary>
        public long Attempts { get; set; }
    }
}