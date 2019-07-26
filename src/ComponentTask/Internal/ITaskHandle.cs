namespace ComponentTask.Internal
{
    internal interface ITaskHandle
    {
        bool IsCompleted { get; }

        DiagTaskTracer DiagTracer { get; }

        bool TryCancel();
    }
}
