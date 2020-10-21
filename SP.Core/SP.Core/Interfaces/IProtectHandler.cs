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
		Task<int> GetLoginAttempts(AccessAttempts attempt, DateTime fromTime);

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		Task<bool> AddLoginAttempt(AccessAttempts accessAttempt);

		/// <summary>
		/// </summary>
		/// <param name="accessAttempt"></param>
		Task<bool> AnalyzeAttempt(AccessAttempts accessAttempt);
	}
}