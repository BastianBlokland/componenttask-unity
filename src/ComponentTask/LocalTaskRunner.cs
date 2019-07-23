using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Exceptions;
using ComponentTask.Internal;

namespace ComponentTask
{
    /// <summary>
    /// TaskRunner that you can 'tick' manually.
    /// </summary>
    public sealed class LocalTaskRunner : ITaskRunner, IDisposable
    {
        private readonly ManualSynchronizationContext context = new ManualSynchronizationContext();
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private readonly IExceptionHandler exceptionHandler;

        private readonly object runningTasksLock = new object();
        private readonly List<ITaskHandle> runningTasks = new List<ITaskHandle>();

        private volatile bool isDisposed;

        /// <summary>
        /// Construct a new instance of the <see cref="LocalTaskRunner"/> class.
        /// </summary>
        /// <param name="exceptionHandler">Handler to use when exception occur in tasks.</param>
        public LocalTaskRunner(IExceptionHandler exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <summary>
        /// Valid in 'DEBUG' only, useful for tests.
        /// </summary>
        [Conditional("DEBUG")]
        public void AssertRunningTaskCount(int expectedCount)
        {
            var realCount = -1;
            lock (this.runningTasksLock)
            {
                realCount = this.runningTasks.Count;
            }

            Debug.Assert(expectedCount == realCount, $"Expected '{expectedCount}' running tasks but found '{realCount}' running tasks");
        }

        /// <inheritdoc/>
        public Task StartTask(Func<Task> taskCreator)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask(taskCreator.Invoke());
            }
        }

        /// <inheritdoc/>
        public Task StartTask(Func<CancellationToken, Task> taskCreator)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask(taskCreator.Invoke(this.cancelSource.Token));
            }
        }

        /// <inheritdoc/>
        public Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask(taskCreator.Invoke(data));
            }
        }

        /// <inheritdoc/>
        public Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask(taskCreator.Invoke(data, this.cancelSource.Token));
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask<TOut>(taskCreator.Invoke());
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask<TOut>(taskCreator.Invoke(this.cancelSource.Token));
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask<TOut>(taskCreator.Invoke(data));
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                return this.WrapTask<TOut>(taskCreator.Invoke(data, this.cancelSource.Token));
            }
        }

        /// <summary>
        /// Execute all the work that was 'scheduled' by the tasks running on this runner.
        /// </summary>
        /// <returns>True if still running work, False if all work has finished.</returns>
        public bool Execute()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            bool tasksRemaining;
            try
            {
                // Execute all the work that was scheduled on this runner.
                using (var contextScope = ContextScope.WithContext(this.context))
                {
                    this.context.Execute();
                }
            }
            finally
            {
                // Remove any tasks that are now finished.
                lock (this.runningTasksLock)
                {
                    for (int i = this.runningTasks.Count - 1; i >= 0; i--)
                    {
                        if (this.runningTasks[i].IsFinished)
                            this.runningTasks.RemoveAt(i);
                    }

                    tasksRemaining = this.runningTasks.Count > 0;
                }
            }

            return tasksRemaining;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.cancelSource.Cancel();
            this.isDisposed = true;

            // Cancel all tasks that are still running.
            lock (this.runningTasksLock)
            {
                foreach (var runningTask in this.runningTasks)
                    runningTask.TryCancel();
                this.runningTasks.Clear();
            }
        }

        private Task WrapTask(Task task)
        {
            if (task is null)
                throw new TaskCreatorReturnedNullException();

            // Handle exceptions that are thrown in the synchronous part.
            if (task.IsFaulted)
            {
                this.exceptionHandler.HandleAll(task.Exception);
                return task;
            }

            if (task.IsCanceled)
            {
                // Log a exception to avoid silent failures.
                this.exceptionHandler.Handle(new ComponentTaskCanceledException());
                return task;
            }

            // Fast path if the task completes synchronously.
            if (task.IsCompleted)
                return task;

            // Create handle and complete it when the given task completes.
            var handle = new TaskHandle(this.exceptionHandler);
            task.ContinueWith(TaskHandle.UpdateFromTask, handle, TaskContinuationOptions.ExecuteSynchronously);

            this.RegisterTaskHandle(handle);
            return handle.Task;
        }

        private Task<T> WrapTask<T>(Task<T> task)
        {
            if (task is null)
                throw new TaskCreatorReturnedNullException();

            // Handle exceptions that are thrown in the synchronous part.
            if (task.IsFaulted)
            {
                this.exceptionHandler.HandleAll(task.Exception);
                return task;
            }

            // Log a 'OperationCanceledException' to avoid silent failures.
            if (task.IsCanceled)
            {
                // Log a exception to avoid silent failures.
                this.exceptionHandler.Handle(new ComponentTaskCanceledException());
                return task;
            }

            // Fast path if the task completes synchronously.
            if (task.IsCompleted)
                return task;

            // Create handle and complete it when the given task completes.
            var handle = new TaskHandle<T>(this.exceptionHandler);
            task.ContinueWith(TaskHandle<T>.UpdateFromTask, handle, TaskContinuationOptions.ExecuteSynchronously);

            this.RegisterTaskHandle(handle);
            return handle.Task;
        }

        private void RegisterTaskHandle(ITaskHandle handle)
        {
            Debug.Assert(handle != null, "Null task-handle provided");
            lock (this.runningTasksLock)
            {
                this.runningTasks.Add(handle);
            }
        }
    }
}
