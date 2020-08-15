using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SP.API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class LoginAttempts : ControllerBase
	{
		private readonly DbContextOptions<Db> db;
		private readonly ILogger<LoginAttempts> log;

		public LoginAttempts(ILogger<LoginAttempts> log, DbContextOptions<Db> db)
		{
			this.log = log;
			this.db = db;
		}

		[HttpPost]
		[Route(nameof(GetLoginAttempts))]
		public async Task<int> GetLoginAttempts([FromBody] Models.LoginAttempts loginAttempt, bool detectIPRange,
			DateTime fromTime)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetLoginAttempts)}.");

			// Open handle to database
			await using Db database = new Db(db);

			// Determine if IP Range block is enabled.
			if (detectIPRange)
			{
				// Match on the first 3 blocks
				return database.LoginAttempts
					.Where(l =>
						l.IpAddress1 == loginAttempt.IpAddress1 &&
						l.IpAddress2 == loginAttempt.IpAddress2 &&
						l.IpAddress3 == loginAttempt.IpAddress3)
					.Count(l => l.EventDate > fromTime);
			}

			// Return results
			return database.LoginAttempts
				.Where(l => l.IpAddress == loginAttempt.IpAddress)
				.Count(l => l.EventDate > fromTime);
		}

		[HttpPost]
		[Route(nameof(Add))]
		public async Task<bool> Add(Models.LoginAttempts loginAttempt)
		{
			log.LogDebug(
				$"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(Add)}.");

			// Open handle to database
			await using Db database = new Db(db);

			database.LoginAttempts.Add(loginAttempt);
			return await database.SaveChangesAsync() > 0;
		}
	}
}