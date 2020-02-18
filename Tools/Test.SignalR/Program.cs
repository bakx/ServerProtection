using System;
using System.Threading.Tasks;
using SP.Models;
using SP.Plugins;

namespace Testing
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            TestSignalR r = new TestSignalR();
            await r.Initialize(null);
            await r.Configure();

            PluginEventArgs pluginEventArgs = new PluginEventArgs {IPAddress = "127.0.0.1", DateTime = DateTime.Now, Details = "Details"};

            await r.LoginAttempt(pluginEventArgs);

            Blocks block = new Blocks
            {
                IpAddress = pluginEventArgs.IPAddress,
                Details = pluginEventArgs.Details,
                City = "City",
                Country = "Country",
                Date = DateTime.Now
            };

            await r.BlockedEvent(block);

        }
    }
}