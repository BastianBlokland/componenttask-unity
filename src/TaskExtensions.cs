using System;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// Extensions for 'System.Task'.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Extension method to suppress the warning you get when you don't await a task in a async
        /// method.
        /// </summary>
        /// <remarks>
        /// Should only be used when you are sure that not-waiting for the task is what you want.
        /// </remarks>
        /// <param name="task">Task to not wait for.</param>
        public static void DontWait(this Task task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));
        }
    }
}
