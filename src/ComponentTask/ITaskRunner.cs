using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComponentTask
{
    /// <summary>
    /// Runner for running tasks.
    /// </summary>
    public interface ITaskRunner
    {
        /// <summary>
        /// Start a task on this runner.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task StartTask(Func<Task> taskCreator);

        /// <summary>
        /// Start a task on this runner.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// runner is disposed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task StartTask(Func<CancellationToken, Task> taskCreator);

        /// <summary>
        /// Start a task on this runner.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data);

        /// <summary>
        /// Start a task on this runner.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// runner is disposed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data);

        /// <summary>
        /// Start a task on this runner.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator);

        /// <summary>
        /// Start a task on this runner.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// runner is disposed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator);

        /// <summary>
        /// Start a task on this runner.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data);

        /// <summary>
        /// Start a task on this runner.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// runner is disposed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the runner gets disposed.
        /// </returns>
        Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data);
    }
}
