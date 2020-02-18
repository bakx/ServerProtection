using System;
using System.Threading.Tasks;
using SP.Models;

namespace Testing
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            TestSignalR r = new TestSignalR();
            await r.Initialize(null);
            await r.Configure();

            LoginAttempts attempt = new LoginAttempts {Id =  1, IpAddress = "127.0.0.1", EventDate = DateTime.Now, Details = "Details"};
            Blocks block = new Blocks { Id = 1, IpAddress = "127.0.0.1", Date = DateTime.Now, Details = "Details", Country = "Country", City = "City", ISP = "ISP", Hostname = "www.hostname.com"};

            await r.LoginAttempt(attempt);
            await r.BlockedEvent(block);
        }
    }
}