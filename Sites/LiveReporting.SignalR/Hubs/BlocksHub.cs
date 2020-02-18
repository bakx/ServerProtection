using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace LiveReporting.SignalR.Hubs
{
    public class BlocksHub : Hub
    {
        public async Task Block(SP.Models.Blocks block)
        {
            await Clients.All.SendCoreAsync("ReportBlock", new object[] { block.Id, block.IpAddress, block.City, block.Country, block.ISP }, CancellationToken.None);
        }
    }
}
