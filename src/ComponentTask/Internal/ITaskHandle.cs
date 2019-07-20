namespace ComponentTask.Internal
{
    internal interface ITaskHandle
    {
        bool IsFinished { get; }

        bool TryCancel();
    }
}