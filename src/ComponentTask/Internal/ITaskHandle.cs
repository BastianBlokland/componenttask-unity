using System.Threading.Tasks;

namespace ComponentTask.Internal
{
    internal interface ITaskHandle
    {
        Task Task { get; }

        bool IsCompleted { get; }

        DiagTaskTracer DiagTracer { get; }

        bool TryCancel();
    }
}
