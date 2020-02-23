using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SP.Models;

namespace SP.Core.Interfaces
{
    public interface IApiHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        Task<List<Blocks>> GetUnblock(int minutes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> AddBlock(Blocks block);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> UpdateBlock(Blocks block);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> StatisticsUpdateBlocks(Blocks block);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <param name="detectIPRange"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        Task<int> GetLoginAttempts(LoginAttempts loginAttempt, bool detectIPRange, DateTime fromTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAttempt"></param>
        /// <returns></returns>
        Task<bool> AddLoginAttempt(LoginAttempts loginAttempt);
    }
}