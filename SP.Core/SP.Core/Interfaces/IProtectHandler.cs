using System;
using System.Threading.Tasks;
using SP.Models;
using SP.Plugins;

namespace SP.Core.Interfaces
{
	public interface IProtectHandler
	{
		/// <summary>
		/// </summary>
		/// <param name="handler"></param>
		void SetApiHandler(IApiHandler handler);

		/// <summary>
		/// </summary>
		/// <param name="attempt"></param>
		/// <param name="fromTime"></param>
		/// <returns></returns>
		Task<int> GetLoginAttempts(LoginAttempts attempt, DateTime fromTime);

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		Task<bool> AddLoginAttempt(LoginAttempts loginAttempt);

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		Task<bool> AnalyzeAttempt(LoginAttempts loginAttempt);
	}
}