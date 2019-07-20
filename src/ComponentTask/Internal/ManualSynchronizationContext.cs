using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ComponentTask.Internal
{
    internal sealed class ManualSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback callback, object state)> workQueue = 
            new ConcurrentQueue<(SendOrPostCallback, object)>();

        public override void Send(SendOrPostCallback callback, object state)
        {
            /* We cannot safely support this blocking api as maybe you want to 'update' this 
            SynchronizationContext from the thread that happens to be calling this method and that would
            cause a dead-lock.
            Luckily there is precedence for this as 'Windows Store Apps' do the same:
            https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext.send */

            throw new NotSupportedException();
        }

        public override void Post(SendOrPostCallback callback, object state) =>
            this.workQueue.Enqueue((callback, state));

        public override SynchronizationContext CreateCopy()
        {
            /* No need to create copies as our instance stays valid. AFAIK this api is not actually
            used anymore. */
            return this;
        }

        public void Execute()
        {
            while (this.workQueue.TryDequeue(out var item))
                item.callback.Invoke(item.state);
        }
    }
}
