using System;
using System.Threading.Tasks;
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

            PluginEventArgs p = new PluginEventArgs {IPAddress = "127.0.0.1", DateTime = DateTime.Now, Details = "Details"};

            await r.LoginAttempt(p);
            await r.BlockedEvent(p);

        }
    }
}