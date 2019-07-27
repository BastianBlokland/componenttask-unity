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

            if (task.IsFaulted)
            {
                if (handle.completeSource.TrySetException(task.Exception))
                {
                    handle.exceptionHandler.HandleAll(task.Exception);
                    handle.diagTracer?.LogCompletedAsFaulted(task.Exception);
                }
            }
            else
            if (task.IsCanceled)
            {
                if (handle.completeSource.TrySetCanceled())
                {
                    // Log a exception to avoid silent failures.
                    handle.exceptionHandler.Handle(new ComponentTaskCanceledException());
                    handle.diagTracer?.LogCompletedAsCanceled();
                }
            }
            else
            if (task.IsCompleted)
            {
                if (handle.completeSource.TrySetResult(default))
                {
                    handle.diagTracer?.LogCompletedAsSuccess(default);
                }
            }
            else
                Debug.Fail("Invalid state");
        };

        private readonly TaskCompletionSource<object> completeSource;
        private readonly IExceptionHandler exceptionHandler;
        private readonly DiagTaskTracer diagTracer;

        public TaskHandle(IExceptionHandler exceptionHandler, DiagTaskTracer diagTracer)
        {
            Debug.Assert(exceptionHandler != null, "No exception handler provided");

            this.completeSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.exceptionHandler = exceptionHandler;
            this.diagTracer = diagTracer;
        }

        public Task Task => this.completeSource.Task;

        public bool IsCompleted => this.completeSource.Task.IsCompleted;

        public DiagTaskTracer DiagTracer => this.diagTracer;

        public bool TryCancel()
        {
            if (this.completeSource.TrySetCanceled())
            {
                this.diagTracer?.LogCanceled();
                return true;
            }

            return false;
        }
    }

    internal sealed class TaskHandle<T> : ITaskHandle
    {
        public static Action<Task<T>, object> UpdateFromTask = (Task<T> task, object state) =>
        {
            Debug.Assert(state is TaskHandle<T>, "State does not contain TaskHandle<T>");
            var handle = (TaskHandle<T>)state;

            if (task.IsFaulted)
            {
                if (handle.completeSource.TrySetException(task.Exception))
                {
                    handle.exceptionHandler.HandleAll(task.Exception);
                    handle.diagTracer?.LogCompletedAsFaulted(task.Exception);
                }
            }
            else
            if (task.IsCanceled)
            {
                if (handle.completeSource.TrySetCanceled())
                {
                    // Log a exception to avoid silent failures.
                    handle.exceptionHandler.Handle(new ComponentTaskCanceledException());
                    handle.diagTracer?.LogCompletedAsCanceled();
                }
            }
            else
            if (task.IsCompleted)
            {
                if (handle.completeSource.TrySetResult(task.Result))
                {
                    handle.diagTracer.LogCompletedAsSuccess(task.Result);
                }
            }
            else
                Debug.Fail("Invalid state");
        };

        private readonly TaskCompletionSource<T> completeSource;
        private readonly IExceptionHandler exceptionHandler;
        private readonly DiagTaskTracer diagTracer;

        public TaskHandle(IExceptionHandler exceptionHandler, DiagTaskTracer diagTracer)
        {
            Debug.Assert(exceptionHandler != null, "No exception handler provided");

            this.completeSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.exceptionHandler = exceptionHandler;
            this.diagTracer = diagTracer;
        }

        public Task Task => this.completeSource.Task;

        public Task<T> TaskWithReturn => this.completeSource.Task;

        public DiagTaskTracer DiagTracer => this.diagTracer;

        public bool IsCompleted => this.completeSource.Task.IsCompleted;

        public bool TryCancel()
        {
            if (this.completeSource.TrySetCanceled())
            {
                this.diagTracer?.LogCanceled();
                return true;
            }

            return false;
        }
    }
}
