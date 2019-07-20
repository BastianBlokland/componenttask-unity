using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when the 'SynchronizationContext.Current' changes unexpectedly,
    /// most likely caused by code that sets 'SynchronizationContext.Current' but does not set it
    /// back to the previous.
    /// </summary>
    public sealed class ContextChangedException : InvalidOperationException
    {
        internal ContextChangedException()
            : base($"Invalid operation: Synchronization context was changed unexpectedly. Did you forget to set the previous context back?")
        {
        }
    }
}
