using System;

namespace ComponentTask
{
    /// <summary>
    /// Interface for a exception handler.
    /// </summary>
    /// <remarks>
    /// Can be used to log exceptions when using a custom <see cref="ComponentTask.LocalTaskRunner"/>.
    /// </remarks>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handle the exception that was thrown.
        /// </summary>
        /// <param name="exception">Exception that was thrown.</param>
        void Handle(Exception exception);
    }
}
