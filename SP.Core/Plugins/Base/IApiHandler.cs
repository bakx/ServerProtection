using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SP.Models;

namespace SP.Plugins
{
    public interface IApiHandler
    {
        /// <summary>
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        Task<List<Blocks>> GetUnblock(int minutes);

        /// <summary>
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> AddBlock(Blocks block);

        /// <summary>
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> UpdateBlock(Blocks block);

        /// <summary>
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<bool> StatisticsUpdateBlocks(Blocks block);

        /// <summary>
        /// </summary>
        /// <param name="accessAttempt"></param>
        /// <param name="detectIPRange"></param>
        /// <param name="fromTime"></param>
        /// <returns></returns>
        Task<int> GetLoginAttempts(AccessAttempts accessAttempt, bool detectIPRange, DateTime fromTime);

        /// <summary>
        /// </summary>
        /// <param name="accessAttempt"></param>
        /// <returns></returns>
        Task<bool> AddLoginAttempt(AccessAttempts accessAttempt);
    }
}