namespace ComponentTask.Internal
{
    internal interface ITaskHandle
    {
        bool IsCompleted { get; }

        bool TryCancel();
    }
}
