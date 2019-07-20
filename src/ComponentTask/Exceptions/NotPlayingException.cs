using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when an api that can only be called while 'Playing' is called in
    /// edit-mode.
    /// </summary>
    public sealed class NotPlayingException : InvalidOperationException
    {
        internal NotPlayingException()
            : base($"Invalid operation: Can only be called in play-mode.")
        {
        }
    }
}
