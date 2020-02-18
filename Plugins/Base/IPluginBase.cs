using System;
using System.Threading.Tasks;

namespace SP.Plugins
{
    public interface IPluginBase
    {
        Task<bool> Initialize(PluginOptions options);
        Task<bool> Configure();

        Task<bool> RegisterLoginAttemptHandler(EventHandler eventHandler);
        Task<bool> RegisterBlockHandler(EventHandler eventHandler);

        // Invoked Event methods
        Task<bool> LoginAttempt(PluginEventArgs pluginEventArgs);
        Task<bool> BlockedEvent(PluginEventArgs pluginEventArgs);
    }
}