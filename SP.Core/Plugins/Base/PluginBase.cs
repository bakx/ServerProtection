using System.Threading.Tasks;
using SP.Models;

namespace SP.Plugins
{
	public class PluginBase : IPluginBase
	{
		public virtual async Task<bool> Initialize(PluginOptions options)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> Configure()
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> RegisterAccessAttemptHandler(IPluginBase.AccessAttempt eventHandler)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> RegisterBlockHandler(IPluginBase.Block eventHandler)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock eventHandler)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> AccessAttemptEvent(AccessAttempts accessAttempt)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> BlockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> UnblockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}
	}
}
