using System.Threading.Tasks;

namespace ComponentTask.Exceptions
{
    /// <summary>
    /// Exception that is attached to tasks returned from <see cref="ITaskRunner"/> when they are
    /// cancelled.
    /// </summary>
    public sealed class ComponentTaskCanceledException : TaskCanceledException
    {
        internal ComponentTaskCanceledException()
            : base($"Component-task was cancelled. Was the component running the task destroyed?")
        {
        }
    }
}
