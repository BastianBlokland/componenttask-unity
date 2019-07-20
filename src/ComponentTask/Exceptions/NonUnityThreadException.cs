using System;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is thrown when an api that can only be called from the unity-thread is called
    /// from a different thread.
    /// </summary>
    public sealed class NonUnityThreadException : InvalidOperationException
    {
        internal NonUnityThreadException()
            : base($"Invalid operation: Has to be executed from the Unity-thread.")
        {
        }
    }
}
