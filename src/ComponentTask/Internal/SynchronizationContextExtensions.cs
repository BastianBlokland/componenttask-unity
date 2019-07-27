using System;
using System.Threading;

namespace ComponentTask.Internal
{
    internal static class SynchronizationContextExtensions
    {
        public static bool IsActive(this SynchronizationContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            return object.ReferenceEquals(SynchronizationContext.Current, context);
        }
    }
}
