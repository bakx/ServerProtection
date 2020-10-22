using System;
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
		private IApiHandler apiHandler;

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
		public ProtectHandler(ILogger<ProtectHandler> log, IConfigurationRoot config)
		{
			this.log = log;

			attempts = config.GetSection("Blocking:Attempts").Get<int>();
			timeSpanMinutes = config.GetSection("Blocking:TimeSpanMinutes").Get<int>();
			detectIPRange = config.GetSection("Blocking:DetectIPRange").Get<bool>();
		}

		/// <summary>
		/// Assign the Api handler
		/// </summary>
		/// <param name="handler"></param>
		public void SetApiHandler(IApiHandler handler)
		{
			apiHandler = handler;
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <param name="fromTime"></param>
		/// <returns></returns>
		public async Task<int> GetLoginAttempts(AccessAttempts accessAttempt, DateTime fromTime)
		{
			return await apiHandler.GetLoginAttempts(accessAttempt, detectIPRange, fromTime);
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		public async Task<bool> AddLoginAttempt(AccessAttempts accessAttempt)
		{
			// Increase statistics
			return await apiHandler.AddLoginAttempt(accessAttempt);
		}

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		/// <returns></returns>
		public async Task<bool> AnalyzeAttempt(AccessAttempts accessAttempt)
		{
			DateTime previousLogins = DateTime.Now.Subtract(new TimeSpan(0, timeSpanMinutes, 0));

			// Determine the block count
			int previousAttempts = await GetLoginAttempts(accessAttempt, previousLogins);

			// Diagnostics
			log.LogDebug($"{accessAttempt.IpAddress} has {previousAttempts} login attempts");

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