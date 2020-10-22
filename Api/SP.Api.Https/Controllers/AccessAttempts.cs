using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SP.Api.Https.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AccessAttempts : ControllerBase
	{
		private readonly DbContextOptions<Db> db;
		private readonly ILogger<AccessAttempts> log;

		public AccessAttempts(ILogger<AccessAttempts> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		[HttpPost]
		[Route(nameof(GetAccessAttempts))]
		public async Task<int> GetAccessAttempts([FromBody] Models.AccessAttempts accessAttempt, bool detectIPRange,
			DateTime fromTime)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetAccessAttempts)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if IP Range block is enabled.
			if (detectIPRange)
			{
				// Match on the first 3 blocks
				return database.AccessAttempts
					.Where(l =>
						l.IpAddress1 == accessAttempt.IpAddress1 &&
						l.IpAddress2 == accessAttempt.IpAddress2 &&
						l.IpAddress3 == accessAttempt.IpAddress3)
					.Count(l => l.EventDate > fromTime);
			}

			// Return results
			return database.AccessAttempts
				.Where(l => l.IpAddress == accessAttempt.IpAddress)
				.Count(l => l.EventDate > fromTime);
		}

		[HttpPost]
		[Route(nameof(Add))]
		public async Task<bool> Add(Models.AccessAttempts accessAttempt)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(Add)}.");

			// Open handle to database
			await using Db database = new Db(db);

			await database.AccessAttempts.AddAsync(accessAttempt);
			return await database.SaveChangesAsync() > 0;
		}
	}
}