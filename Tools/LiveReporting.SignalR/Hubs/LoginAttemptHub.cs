using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace LiveReporting.SignalR.Hubs
{
    public class LoginAttemptHub : Hub
    {
        public async Task LoginAttempt(SP.Models.LoginAttempts attempt)
        {
            await Clients.All.SendCoreAsync("ReportLoginAttempt", new object[] {attempt.Id, attempt.IpAddress, attempt.EventDate, attempt.Details}, CancellationToken.None);
        }
    }
}
