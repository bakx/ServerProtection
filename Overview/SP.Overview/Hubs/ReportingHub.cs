using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SP.Models;
using SP.Overview.Helpers;

namespace SP.Overview.Hubs
{
	public class ReportingHub : Hub
	{
		/// <summary>
		/// </summary>
		/// <param name="attempt"></param>
		/// <returns></returns>
		public async Task AccessAttempt(AccessAttempts attempt)
		{
			await Clients.All.SendCoreAsync("ReportAccessAttempt",
				new object[] {attempt.Id, attempt.IpAddress, attempt.EventDate, attempt.Details, AttackTypeString.GetName(attempt.AttackType)},
				CancellationToken.None);
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task Block(Blocks block)
		{
			await Clients.All.SendCoreAsync("ReportBlock",
				new object[]
					{block.Id, block.Date, block.Details, block.IpAddress, block.City, block.Country, block.ISP, AttackTypeString.GetName(block.AttackType)},
				CancellationToken.None);
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task Unblock(Blocks block)
		{
			await Clients.All.SendCoreAsync("ReportUnblock",
				new object[]
					{block.Id, block.Date, block.Details, block.IpAddress, block.City, block.Country, block.ISP, AttackTypeString.GetName(block.AttackType)},
				CancellationToken.None);
		}
	}
}