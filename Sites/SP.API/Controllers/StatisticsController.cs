using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SP.Models;

namespace SP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> log;

        public StatisticsController(ILogger<StatisticsController> log)
        {
            this.log = log;
        }

        [HttpPost]
        [Route(nameof(UpdateBlock))]
        public async Task<bool> UpdateBlock(Blocks block)
        {
            log.LogDebug($"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

            // Open handle to database
            await using Db db = new Db();

            // Determine if this country has been blocked before
            StatisticsBlocks blocks = db.StatisticsBlocks.FirstOrDefault(s =>
                s.Country == block.Country && s.City == block.City && s.ISP == block.ISP);

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
            return await db.SaveChangesAsync() > 0;
        }
    }
}
