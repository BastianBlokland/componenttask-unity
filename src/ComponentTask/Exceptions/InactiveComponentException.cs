using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when attempting to execute a operation on a inactive component.
    /// </summary>
    public sealed class InactiveComponentException : InvalidOperationException
    {
        internal InactiveComponentException(string message)
            : base($"Invalid operation: {message}")
        {
        }
    }
}
