using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace LiveReporting.SignalR.Hubs
{
    public class ReportingHub : Hub
    {
        public async Task LoginAttempt(SP.Models.LoginAttempts attempt)
        {
            await Clients.All.SendCoreAsync("ReportLoginAttempt", new object[] {attempt.Id, attempt.IpAddress, attempt.EventDate, attempt.Details}, CancellationToken.None);
        }

        public async Task Block(SP.Models.Blocks block)
        {
            await Clients.All.SendCoreAsync("ReportBlock", new object[] { block.Id, block.IpAddress, block.City, block.Country, block.ISP }, CancellationToken.None);
        }
    }
}
