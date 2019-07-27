using System;
using System.Threading;
using ComponentTask.Exceptions;

namespace ComponentTask.Internal
{
    internal readonly struct ContextScope : IDisposable
    {
        private readonly SynchronizationContext previousContext;
        private readonly SynchronizationContext currentContext;

        private ContextScope(SynchronizationContext previousContext, SynchronizationContext currentContext)
        {
            this.previousContext = previousContext;
            this.currentContext = currentContext;
        }

        public void Dispose()
        {
            // Sanity check that the static build method was used instead of the default constructor.
            if (this.currentContext is null)
                return;

            /* Verify that the scope is still (or again) the context we are expecting. This could
            for example show if you forget to set back the previous context in any code that changes
            the context. */
            if (!this.currentContext.IsActive())
                throw new ContextChangedException();

            // Set the synchronization context back.
            SynchronizationContext.SetSynchronizationContext(this.previousContext);
        }

        public static ContextScope WithContext(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext is null)
                throw new ArgumentNullException(nameof(synchronizationContext));

            // 'Capture' the previous context.
            var previousContext = SynchronizationContext.Current;

            // The the new context as active.
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);

            // Create a scope to set it back on dispose.
            return new ContextScope(previousContext, synchronizationContext);
        }
    }
}
