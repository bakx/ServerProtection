using System;
using System.Threading;
using System.Threading.Tasks;

namespace SP.Core.Interfaces
{
    public interface ICoreService
    {
        event EventHandler LoginAttemptEvent;
        event EventHandler BlockEvent;
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        void Dispose();
    }
}