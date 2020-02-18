using System;
using System.Threading.Tasks;
using SP.Models;

namespace SP.Core.Interfaces
{
    public interface IProtectHandler
    {
        /// <summary>
        /// </summary>
        /// <param name="attempt"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        Task<int> GetLoginAttempts(LoginAttempts attempt, DateTime fromTime);

        /// <summary>
        /// </summary>
        /// <param name="attempt"></param>
        Task<bool> AnalyzeAttempt(LoginAttempts attempt);
    }
}