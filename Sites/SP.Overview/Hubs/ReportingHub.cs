using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SP.Models;

namespace SP.Overview.Hubs
{
    public class ReportingHub : Hub
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attempt"></param>
        /// <returns></returns>
        public async Task LoginAttempt(LoginAttempts attempt)
        {
            await Clients.All.SendCoreAsync("ReportLoginAttempt",
                new object[] {attempt.Id, attempt.IpAddress, attempt.EventDate, attempt.Details},
                CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public async Task Block(Blocks block)
        {
            await Clients.All.SendCoreAsync("ReportBlock",
                new object[] {block.Id, block.IpAddress, block.City, block.Country, block.ISP}, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public async Task Unblock(Blocks block)
        {
            await Clients.All.SendCoreAsync("ReportUnblock",
                new object[] { block.Id, block.IpAddress, block.City, block.Country, block.ISP }, CancellationToken.None);
        }
    }
}