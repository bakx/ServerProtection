using System.Threading.Tasks;
using SP.Models;

namespace SP.Plugins
{
    public interface IPluginBase
    {
        public delegate void Block(LoginAttempts loginAttempt);

        public delegate void LoginAttempt(LoginAttempts loginAttempt);

        public delegate void Unblock(Blocks block);

        Task<bool> Initialize(PluginOptions options);
        Task<bool> Configure();

        Task<bool> RegisterLoginAttemptHandler(LoginAttempt eventHandler);
        Task<bool> RegisterBlockHandler(Block eventHandler);
        Task<bool> RegisterUnblockHandler(Unblock eventHandler);

        // Invoked Event methods
        Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt);
        Task<bool> BlockEvent(Blocks block);
        Task<bool> UnblockEvent(Blocks block);
    }
}