using System;

namespace ComponentTask.Internal
{
    internal static class ExceptionHandlerExtensions
    {
        public static void HandleAll(this IExceptionHandler handler, AggregateException aggregateException)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));
            if (aggregateException is null)
                throw new ArgumentNullException(nameof(aggregateException));

            foreach (var exception in aggregateException.Flatten().InnerExceptions)
                handler.Handle(exception);
        }
    }
}
