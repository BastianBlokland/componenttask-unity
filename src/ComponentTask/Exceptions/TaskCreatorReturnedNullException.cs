using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when a task-creator returns null.
    /// </summary>
    public sealed class TaskCreatorReturnedNullException : InvalidOperationException
    {
        internal TaskCreatorReturnedNullException()
            : base($"Invalid operation: Null was returned from a task-creator function.")
        {
        }
    }
}
