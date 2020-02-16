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
        Task<bool> BlockedEvent(PluginEventArgs pluginEventArgs);
    }
}