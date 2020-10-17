using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SP.Models;

namespace SP.Api.Https.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class StatisticsController : ControllerBase
	{
		private readonly DbContextOptions<Db> db;
		private readonly ILogger<StatisticsController> log;

		public StatisticsController(ILogger<StatisticsController> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		[HttpPost]
		[Route(nameof(UpdateBlock))]
		public async Task<bool> UpdateBlock(Blocks block)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if this country has been blocked before
			StatisticsBlocks blocks = database.StatisticsBlocks.FirstOrDefault(s =>
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

				await database.StatisticsBlocks.AddAsync(statisticsBlocks);
			}

			// Save changes
			return await database.SaveChangesAsync() > 0;
		}
	}
}