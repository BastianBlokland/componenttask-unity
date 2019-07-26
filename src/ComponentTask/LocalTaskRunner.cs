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
        public Task StartTask(Func<Task> taskCreator) =>
            this.StartTask(taskCreator, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask(Func{Task})"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task StartTask(
            Func<Task> taskCreator,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked();
                return this.WrapTask(taskCreator.Invoke(), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task StartTask(Func<CancellationToken, Task> taskCreator) =>
            this.StartTask(taskCreator, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask(Func{CancellationToken, Task})"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task StartTask(
            Func<CancellationToken, Task> taskCreator,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked();
                return this.WrapTask(taskCreator.Invoke(this.cancelSource.Token), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data) =>
            this.StartTask(taskCreator, data, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TIn}(Func{TIn, Task}, TIn)"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task StartTask<TIn>(
            Func<TIn, Task> taskCreator,
            TIn data,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked(data);
                return this.WrapTask(taskCreator.Invoke(data), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data) =>
            this.StartTask(taskCreator, data, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TIn}(Func{TIn, CancellationToken, Task}, TIn)"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task StartTask<TIn>(
            Func<TIn, CancellationToken, Task> taskCreator,
            TIn data,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked(data);
                return this.WrapTask(taskCreator.Invoke(data, this.cancelSource.Token), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator) =>
            this.StartTask(taskCreator, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TOut}(Func{Task{TOut}})"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task<TOut> StartTask<TOut>(
            Func<Task<TOut>> taskCreator,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked();
                return this.WrapTask<TOut>(taskCreator.Invoke(), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator) =>
            this.StartTask(taskCreator, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TOut}(Func{CancellationToken, Task{TOut}})"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task<TOut> StartTask<TOut>(
            Func<CancellationToken, Task<TOut>> taskCreator,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked();
                return this.WrapTask<TOut>(taskCreator.Invoke(this.cancelSource.Token), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data) =>
            this.StartTask(taskCreator, data, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TIn, TOut}(Func{TIn, Task{TOut}}, TIn)"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task<TOut> StartTask<TIn, TOut>(
            Func<TIn, Task<TOut>> taskCreator,
            TIn data,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked(data);
                return this.WrapTask<TOut>(taskCreator.Invoke(data), diagTracer);
            }
        }

        /// <inheritdoc/>
        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data) =>
            this.StartTask(taskCreator, data, logger: null);

        /// <inheritdoc cref="ITaskRunner.StartTask{TIn, TOut}(Func{TIn, CancellationToken, Task{TOut}}, TIn)"/>
        /// <param name="logger">Optional logger to output diagnostic messages to.</param>
        public Task<TOut> StartTask<TIn, TOut>(
            Func<TIn, CancellationToken, Task<TOut>> taskCreator,
            TIn data,
            IDiagnosticLogger logger)
        {
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

            // Activate our context and wrap the task.
            using (var contextScope = ContextScope.WithContext(this.context))
            {
                var diagTracer = logger == null ? null : DiagTaskTracer.Create(logger, taskCreator);
                diagTracer?.LogInvoked(data);
                return this.WrapTask<TOut>(taskCreator.Invoke(data, this.cancelSource.Token), diagTracer);
            }
        }

        /// <summary>
        /// Execute all the work that was 'scheduled' by the tasks running on this runner.
        /// </summary>
        public void Execute()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(nameof(LocalTaskRunner));

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
                        if (this.runningTasks[i].IsCompleted)
                            this.runningTasks.RemoveAt(i);
                    }
                }
            }
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

        /// <summary>
        /// Invoke given action on each running task.
        /// </summary>
        /// <summary>
        /// Note: Internal as it exposes allot of implementation details.
        /// </summary>
        /// <param name="action">Action to invoke on each running task.</param>
        internal void ForAllRunningTasks(Action<ITaskHandle> action) =>
            this.runningTasks.LockedInvoke(this.runningTasksLock, action);

        private Task WrapTask(Task task, DiagTaskTracer diagTracer)
        {
            if (task is null)
                throw new TaskCreatorReturnedNullException();

            // Handle exceptions that are thrown in the synchronous part.
            if (task.IsFaulted)
            {
                diagTracer?.LogCompletedSynchronouslyAsFaulted(task.Exception);
                this.exceptionHandler.HandleAll(task.Exception);
                return task;
            }

            if (task.IsCanceled)
            {
                diagTracer?.LogCompletedSynchronouslyAsCanceled();

                // Log a exception to avoid silent failures.
                this.exceptionHandler.Handle(new ComponentTaskCanceledException());
                return task;
            }

            // Fast path if the task completes synchronously.
            if (task.IsCompleted)
            {
                diagTracer?.LogCompletedSynchronouslyAsSuccess(default);

                return task;
            }

            diagTracer?.LogStartRunning();

            // Create handle and complete it when the given task completes.
            var handle = new TaskHandle(this.exceptionHandler, diagTracer);
            task.ContinueWith(TaskHandle.UpdateFromTask, handle, TaskContinuationOptions.ExecuteSynchronously);

            this.RegisterTaskHandle(handle);
            return handle.Task;
        }

        private Task<T> WrapTask<T>(Task<T> task, DiagTaskTracer diagTracer)
        {
            if (task is null)
                throw new TaskCreatorReturnedNullException();

            // Handle exceptions that are thrown in the synchronous part.
            if (task.IsFaulted)
            {
                diagTracer?.LogCompletedSynchronouslyAsFaulted(task.Exception);
                this.exceptionHandler.HandleAll(task.Exception);
                return task;
            }

            // Log a 'OperationCanceledException' to avoid silent failures.
            if (task.IsCanceled)
            {
                diagTracer?.LogCompletedSynchronouslyAsCanceled();

                // Log a exception to avoid silent failures.
                this.exceptionHandler.Handle(new ComponentTaskCanceledException());
                return task;
            }

            // Fast path if the task completes synchronously.
            if (task.IsCompleted)
            {
                diagTracer?.LogCompletedSynchronouslyAsSuccess(task.Result);
                return task;
            }

            diagTracer?.LogStartRunning();

            // Create handle and complete it when the given task completes.
            var handle = new TaskHandle<T>(this.exceptionHandler, diagTracer);
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
