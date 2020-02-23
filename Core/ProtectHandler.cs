using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SP.Core.Interfaces;
using SP.Models;

namespace SP.Core
{
    public class ProtectHandler : IProtectHandler
    {
        private readonly IApiHandler apiHandler;

        private readonly int attempts;
        private readonly bool detectIPRange;

        // Diagnostics
        private readonly ILogger<ProtectHandler> log;

        // Configuration settings
        private readonly int timeSpanMinutes;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        /// <param name="apiHandler"></param>
        public ProtectHandler(ILogger<ProtectHandler> log, IConfigurationRoot config, IApiHandler apiHandler)
        {
            this.log = log;
            this.apiHandler = apiHandler;

            attempts = config.GetSection("Blocking:Attempts").Get<int>();
            timeSpanMinutes = config.GetSection("Blocking:TimeSpanMinutes").Get<int>();
            detectIPRange = config.GetSection("Blocking:DetectIPRange").Get<bool>();
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        public async Task<int> GetLoginAttempts(LoginAttempts loginAttempt, DateTime fromTime)
        {
            return await apiHandler.GetLoginAttempts(loginAttempt, detectIPRange, fromTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        public async Task<bool> AddLoginAttempt(LoginAttempts loginAttempt)
        {
            // Increase statistics
            return await apiHandler.AddLoginAttempt(loginAttempt);
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <returns></returns>
        public async Task<bool> AnalyzeAttempt(LoginAttempts loginAttempt)
        {
            DateTime previousLogins = DateTime.Now.Subtract(new TimeSpan(0, timeSpanMinutes, 0));

            // Determine the block count
            int previousAttempts = await GetLoginAttempts(loginAttempt, previousLogins);

            // Diagnostics
            log.LogDebug($"{loginAttempt.IpAddress} has {previousAttempts} login attempts");

            // If the amount of attempts exceed the configured value, return true to indicate that this IP should be blocked
            if (previousAttempts >= attempts)
            {
                return await Task.FromResult(true);
            }

            // Indicate that the IP does not have to be blocked
            return await Task.FromResult(false);
        }
    }
}