using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask;
using ComponentTask.Internal;

namespace UnityEngine
{
    /// <summary>
    /// Extensions for <see cref="UnityEngine.Component"/>.
    /// </summary>
    public static class ComponentExtensions
    {
        private static List<MonoBehaviourTaskRunner> monoBehaviourRunners = new List<MonoBehaviourTaskRunner>();

        /// <summary>
        /// Start a task scoped to the given component.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task StartTask(this Component component, Func<Task> taskCreator)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// component gets destroyed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task StartTask(this Component component, Func<CancellationToken, Task> taskCreator)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task StartTask<TIn>(this Component component, Func<TIn, Task> taskCreator, TIn data)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator, data);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// component gets destroyed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task StartTask<TIn>(this Component component, Func<TIn, CancellationToken, Task> taskCreator, TIn data)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator, data);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task<TOut> StartTask<TOut>(this Component component, Func<Task<TOut>> taskCreator)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// component gets destroyed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task<TOut> StartTask<TOut>(this Component component, Func<CancellationToken, Task<TOut>> taskCreator)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task<TOut> StartTask<TIn, TOut>(this Component component, Func<TIn, Task<TOut>> taskCreator, TIn data)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator, data);
        }

        /// <summary>
        /// Start a task scoped to the given component.
        /// <see cref="CancellationToken"/> that is passed to the task-creator is cancelled when the
        /// component gets destroyed, this can be usefull for cancelling external processes.
        /// </summary>
        /// <remarks>
        /// The task will run 'on' the component, meaning that the task gets paused when the component
        /// is disabled and the task will get cancelled when the component is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.TaskCreatorReturnedNullException">
        /// Thrown when null is returned from the 'taskCreator'.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.ComponentTaskCanceledException">
        /// Thrown when awaiting a component-task that gets cancelled. Can happen if you are awaiting
        /// a component that gets destroyed.
        /// </exception>
        /// <param name="component">Component to run the task 'on'.</param>
        /// <param name="taskCreator">Function for creating the task.</param>
        /// <returns>
        /// Task that completes when the original task completes or when the component gets destroyed.
        /// </returns>
        public static Task<TOut> StartTask<TIn, TOut>(this Component component, Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component).StartTask(taskCreator, data);
        }

        /// <summary>
        /// Get a <see cref="ITaskRunner"/> for the given component.
        /// </summary>
        /// <remarks>
        /// If a existing runner exists for the component then that is returned, otherwise a new
        /// runner is created.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <param name="component">Component to get the runner for.</param>
        /// <returns><see cref="ITaskRunner"/> scoped to the given component.</returns>
        public static ITaskRunner GetTaskRunner(this Component component)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(component, nameof(component));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Start the task on a runner for the component.
            return GetTaskRunnerUnchecked(component);
        }

        internal static ITaskRunner GetTaskRunnerUnchecked(this Component component)
        {
            var owner = component.gameObject;

            // Check if there is a existing runner for the same scope.
            owner.GetComponents<MonoBehaviourTaskRunner>(monoBehaviourRunners);
            foreach (var runner in monoBehaviourRunners)
            {
                // If there is a runner for that component then return that.
                if (runner.ComponentToFollow == component)
                    return runner;
            }

            // Otherwise create a new runner for this component.
            var newRunner = owner.AddComponent<MonoBehaviourTaskRunner>();
            newRunner.ComponentToFollow = component;
            newRunner.hideFlags = HideFlags.HideInInspector;
            return newRunner;
        }
    }
}
