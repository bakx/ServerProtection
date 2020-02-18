using System.Linq;
using System.Threading.Tasks;
using SP.Models;

namespace SP.Core
{
    public static class Statistics
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateBlocks(Blocks block)
        {
            // Open handle to database
            await using Db db = new Db();

            // Determine if this country has been blocked before
            StatisticsBlocks blocks = db.StatisticsBlocks.FirstOrDefault(s => s.Country == block.Country && s.City == block.City && s.ISP == block.ISP);

            if (blocks != null)
            {
                blocks.Attempts++;
            }
            else
            {
                StatisticsBlocks statisticsBlocks = new StatisticsBlocks
                {
                    Country = block.Country, 
                    ISP = block.ISP, 
                    City = block.City, 
                    Attempts = 1
                };

                db.StatisticsBlocks.Add(statisticsBlocks);
            }

            // Save changes
            await db.SaveChangesAsync();
        }
    }
}