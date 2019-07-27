using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when attempting to execute a operation on a inactive gameobject.
    /// </summary>
    public sealed class InactiveGameObjectException : InvalidOperationException
    {
        internal InactiveGameObjectException(string message)
            : base($"Invalid operation: {message}")
        {
        }
    }
}
