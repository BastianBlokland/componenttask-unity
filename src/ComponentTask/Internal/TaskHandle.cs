using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ComponentTask.Exceptions;

namespace ComponentTask.Internal
{
    internal sealed class TaskHandle : ITaskHandle
    {
        public static Action<Task, object> UpdateFromTask = (Task task, object state) =>
        {
            Debug.Assert(state is TaskHandle, "State does not contain TaskHandle");
            var handle = (TaskHandle)state;

            handle.isFinished = true;
            if (task.IsFaulted)
            {
                try
                {
                    handle.exceptionHandler.HandleAll(task.Exception);
                }
                finally
                {
                    handle.completeSource.TrySetException(task.Exception);
                }
            }
            else
            if (task.IsCanceled)
            {
                try
                {
                    // Log a exception to avoid silent failures.
                    handle.exceptionHandler.Handle(new ComponentTaskCanceledException());
                }
                finally
                {
                    handle.completeSource.TrySetCanceled();
                }
            }
            else
            if (task.IsCompleted)
                handle.completeSource.TrySetResult(default);
            else
                Debug.Fail("Invalid state");
        };

        private readonly TaskCompletionSource<object> completeSource;
        private readonly IExceptionHandler exceptionHandler;

        private volatile bool isFinished;

        public TaskHandle(IExceptionHandler exceptionHandler)
        {
            Debug.Assert(exceptionHandler != null, "No exception handler provided");

            this.completeSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.exceptionHandler = exceptionHandler;
        }

        public Task Task => this.completeSource.Task;

        public bool IsFinished => this.isFinished;

        public bool TryCancel()
        {
            this.isFinished = true;
            return this.completeSource.TrySetCanceled();
        }
    }

    internal sealed class TaskHandle<T> : ITaskHandle
    {
        public static Action<Task<T>, object> UpdateFromTask = (Task<T> task, object state) =>
        {
            Debug.Assert(state is TaskHandle<T>, "State does not contain TaskHandle<T>");
            var handle = (TaskHandle<T>)state;

            handle.isFinished = true;
            if (task.IsFaulted)
            {
                try
                {
                    handle.exceptionHandler.HandleAll(task.Exception);
                }
                finally
                {
                    handle.completeSource.TrySetException(task.Exception);
                }
            }
            else
            if (task.IsCanceled)
            {
                try
                {
                    // Log a exception to avoid silent failures.
                    handle.exceptionHandler.Handle(new ComponentTaskCanceledException());
                }
                finally
                {
                    handle.completeSource.TrySetCanceled();
                }
            }
            else
            if (task.IsCompleted)
                handle.completeSource.TrySetResult(task.Result);
            else
                Debug.Fail("Invalid state");
        };

        private readonly TaskCompletionSource<T> completeSource;
        private readonly IExceptionHandler exceptionHandler;

        private volatile bool isFinished;

        public TaskHandle(IExceptionHandler exceptionHandler)
        {
            Debug.Assert(exceptionHandler != null, "No exception handler provided");

            this.completeSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.exceptionHandler = exceptionHandler;
        }

        public Task<T> Task => this.completeSource.Task;

        public bool IsFinished => this.isFinished;

        public bool TryCancel()
        {
            this.isFinished = true;
            return this.completeSource.TrySetCanceled();
        }
    }
}
