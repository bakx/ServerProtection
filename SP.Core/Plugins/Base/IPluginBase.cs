using System.Threading.Tasks;
using SP.Models;

namespace SP.Plugins
{
    public interface IPluginBase
    {
        public delegate void Block(AccessAttempts accessAttempt);

        public delegate void AccessAttempt(AccessAttempts accessAttempt);

        public delegate void Unblock(Blocks block);

        Task<bool> Initialize(PluginOptions options);
        Task<bool> Configure();

        Task<bool> RegisterAccessAttemptHandler(AccessAttempt eventHandler);
        Task<bool> RegisterBlockHandler(Block eventHandler);
        Task<bool> RegisterUnblockHandler(Unblock eventHandler);

        // Invoked Event methods
        Task<bool> AccessAttemptEvent(AccessAttempts accessAttempt);
        Task<bool> BlockEvent(Blocks block);
        Task<bool> UnblockEvent(Blocks block);
    }
}