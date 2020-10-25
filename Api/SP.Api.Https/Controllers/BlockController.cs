using System;
using System.Collections.Generic;
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
	public class BlockController : ControllerBase
	{
		private readonly DbContextOptions<Db> db;
		private readonly ILogger<BlockController> log;

		public BlockController(ILogger<BlockController> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		/// <summary>
		/// </summary>
		/// <param name="minutes"></param>
		/// <returns></returns>
		[HttpGet]
		[Route(nameof(GetUnblocks))]
		public async Task<List<Blocks>> GetUnblocks(int minutes = 30)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetUnblocks)}. Parameters: minutes {minutes} ");

			// Open handle to database
			await using Db database = new Db(db);

			return database.Blocks.Where(b => b.IsBlocked == 1)
				.ToListAsync().Result.Where(b =>
					b.EventDate < DateTime.Now.Subtract(new TimeSpan(0, minutes, 0)) &&
					b.IsBlocked == 1
				).ToList();
		}

		[HttpPost]
		[Route(nameof(AddBlock))]
		public async Task<bool> AddBlock(Blocks block)
		{
			log.LogDebug($"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(AddBlock)}.");

			// Open handle to database
			await using Db database = new Db(db);

			await database.Blocks.AddAsync(block);
			return await database.SaveChangesAsync() > 0;
		}

		[HttpPost]
		[Route(nameof(UpdateBlock))]
		public async Task<bool> UpdateBlock(Blocks block)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(UpdateBlock)}.");

			// Open handle to database
			await using Db database = new Db(db);

			Blocks blocks = database.Blocks.SingleOrDefault(b => b.Id == block.Id);

			// If the entry cannot be found, ignore the update
			if (blocks == null)
			{
				return false;
			}

			// Overwrite block details
			blocks.City = block.City;
			blocks.Country = block.Country;
			blocks.EventDate = block.EventDate;
			blocks.Hostname = block.Hostname;
			blocks.Details = block.Details;
			blocks.IpAddress = block.IpAddress;
			blocks.IpAddress1 = block.IpAddress1;
			blocks.IpAddress2 = block.IpAddress2;
			blocks.IpAddress3 = block.IpAddress3;
			blocks.IpAddress4 = block.IpAddress4;
			blocks.FirewallRuleName = block.FirewallRuleName;
			blocks.IsBlocked = block.IsBlocked;
			blocks.AttackType = block.AttackType;

			return await database.SaveChangesAsync() > 0;
		}
	}
}