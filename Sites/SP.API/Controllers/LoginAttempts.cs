using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginAttempts : ControllerBase
    {
        private readonly ILogger<LoginAttempts> log;

        public LoginAttempts(ILogger<LoginAttempts> log)
        {
            this.log = log;
        }

        [HttpPost]
        [Route(nameof(GetLoginAttempts))]
        public async Task<int> GetLoginAttempts([FromBody] Models.LoginAttempts loginAttempt, bool detectIPRange, DateTime fromTime)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(GetLoginAttempts)}.");

            // Open handle to database
            await using Db db = new Db();

            // Determine if IP Range block is enabled.
            if (detectIPRange)
            {
                // Match on the first 3 blocks
                return db.LoginAttempts
                    .Where(l =>
                        l.IpAddress1 == loginAttempt.IpAddress1 &&
                        l.IpAddress2 == loginAttempt.IpAddress2 &&
                        l.IpAddress3 == loginAttempt.IpAddress3)
                    .AsEnumerable()
                    .Count(l => l.EventDate > fromTime);
            }

            // Return results
            return db.LoginAttempts
                .Where(l => l.IpAddress == loginAttempt.IpAddress)
                .AsEnumerable()
                .Count(l => l.EventDate > fromTime);
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<bool> Add(Models.LoginAttempts loginAttempt)
        {
            log.LogDebug(
                $"Received call from {Request.HttpContext.Connection.RemoteIpAddress} to {nameof(Add)}.");

            // Open handle to database
            await using Db db = new Db();
            db.LoginAttempts.Add(loginAttempt);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
