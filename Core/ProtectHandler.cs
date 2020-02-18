using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SP.Core.Interfaces;
using SP.Models;
using SP.Plugins;

namespace SP.Core
{
    public class ProtectHandler : IProtectHandler
    {
        private readonly int attempts;
        private readonly bool blockIPRange;
        private readonly bool detectIPRange;

        // Diagnostics
        private readonly ILogger<ProtectHandler> log;

        // Configuration settings
        private readonly int timeSpanMinutes;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        public ProtectHandler(ILogger<ProtectHandler> log, IConfigurationRoot config)
        {
            this.log = log;

            attempts = config.GetSection("Blocking:Attempts").Get<int>();
            timeSpanMinutes = config.GetSection("Blocking:TimeSpanMinutes").Get<int>();
            detectIPRange = config.GetSection("Blocking:DetectIPRange").Get<bool>();
            blockIPRange = config.GetSection("Blocking:BlockIPRange").Get<bool>();
        }

        /// <summary>
        /// </summary>
        /// <param name="attempt"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        public async Task<int> GetLoginAttempts(LoginAttempts attempt, DateTime fromTime)
        {
            // Open handle to database
            await using Db db = new Db();

            // Determine if IP Range block is enabled.
            if (detectIPRange)
            {
                // Match on the first 3 blocks
                var z = attempt.IpAddress.Split('.');
                var a = $"{z[0]}.{z[1]}.{z[2]}";

                log.LogDebug($"{nameof(detectIPRange)} is not yet supported");
            }


            // Return results
            return db.LoginAttempts
                .Where(l => l.IpAddress == attempt.IpAddress)
                .AsEnumerable()
                .Count(l => l.EventDate > fromTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public async Task<bool> AnalyzeAttempt(PluginEventArgs args)
        {
            // Convert arguments to attempt
            LoginAttempts attempt = new LoginAttempts
            {
                IpAddress = args.IPAddress,
                Details = args.Details,
                EventDate = args.DateTime
            };

            // Open handle to database
            await using Db db = new Db();

            // Add the login attempt
            db.LoginAttempts.Add(attempt);

            // Save changes
            await db.SaveChangesAsync();

            // Check if the amount of login attempts exceeds the configured values
            DateTime previousLogins = DateTime.Now.Subtract(new TimeSpan(0, timeSpanMinutes, 0));

            // Determine the block count
            int previousAttempts = await GetLoginAttempts(attempt, previousLogins);

            if (previousAttempts > attempts)
            {
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }
    }
}