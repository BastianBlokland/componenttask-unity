using System;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Exceptions;
using UnityEngine;

namespace ComponentTask.Internal
{
    internal sealed class MonoBehaviourTaskRunner :
        MonoBehaviour, ITaskRunner, IExceptionHandler, IDiagnosticLogger
    {
        private readonly LocalTaskRunner taskRunner;

        private bool isPaused;

        public MonoBehaviourTaskRunner()
        {
            this.taskRunner = new LocalTaskRunner(exceptionHandler: this);
        }

        public TaskRunOptions RunOptions { get; set; }

        public Component ComponentToFollow { get; set; }

        private bool DiagnosticLogging =>
            (this.RunOptions & TaskRunOptions.DiagnosticLogging) == TaskRunOptions.DiagnosticLogging;

        private bool UpdateWhileComponentDisabled =>
            (this.RunOptions & TaskRunOptions.UpdateWhileComponentDisabled) == TaskRunOptions.UpdateWhileComponentDisabled;

        public Task StartTask(Func<Task> taskCreator)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, logger);
        }

        public Task StartTask(Func<CancellationToken, Task> taskCreator)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, logger);
        }

        public Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, data, logger);
        }

        public Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, data, logger);
        }

        public Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, logger);
        }

        public Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, logger);
        }

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, data, logger);
        }

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data)
        {
            this.ThrowForInvalidState();

            var logger = this.DiagnosticLogging ? this as IDiagnosticLogger : null;
            return this.taskRunner.StartTask(taskCreator, data, logger);
        }

        private void ThrowForInvalidState()
        {
            // Verify that this runner has not been destroyed.
            if (!this)
                throw new UnityEngine.MissingReferenceException("Task runner has been destroyed but you are still trying to access it.");

            // Verify that our gameobject is active.
            if (!this.gameObject.activeInHierarchy)
                throw new InactiveGameObjectException($"Unable to start task: GameObject '{this.gameObject.name}' is inactive.");

            // If we are following a component then verify that the component is not inactive.
            if (ComponentToFollow is UnityEngine.Behaviour behaviour)
            {
                if (!this.UpdateWhileComponentDisabled && !behaviour.enabled)
                    throw new InactiveComponentException($"Unable to start task: Component '{behaviour.GetType().Name}' is inactive.");
            }
        }

        // Dynamically called from the Unity runtime.
        private void OnDisable()
        {
            /* Unfortunately this is also called when the gameobject is destroyed, so far i have not
            found any way to (reliably) know if we are disabled or destroy. In practice this means
            that you will see a 'Paused' log before every 'Canceled'. */
            if (!this.isPaused)
            {
                this.isPaused = true;
                this.LogPause();
            }
        }

        // Dynamically called from the Unity runtime.
        private void LateUpdate()
        {
            // Check if we have a 'ComponentToFollow' assigned.
            if (this.ComponentToFollow is null)
            {
                // If not then always just execute the runner.
                this.Execute();
            }
            else
            {
                // If the component we are following has been destroyed then we destroy ourselves.
                if (!this.ComponentToFollow)
                    this.Destroy();
                else
                {
                    // If the component is a 'Behaviour' then we update when its enabled.
                    if (ComponentToFollow is UnityEngine.Behaviour behaviour)
                    {
                        if (this.UpdateWhileComponentDisabled || behaviour.enabled)
                        {
                            this.Execute();
                        }
                        else
                        {
                            if (!this.isPaused)
                            {
                                this.LogPause();
                                this.isPaused = true;
                            }
                        }
                    }
                    else
                    {
                        // Otherwise we always update.
                        this.Execute();
                    }
                }
            }
        }

        // Dynamically called from the Unity runtime.
        private void OnDestroy() => this.taskRunner.Dispose();

        private void Execute()
        {
            if (this.isPaused)
            {
                this.LogResume();
                this.isPaused = false;
            }

            this.taskRunner.Execute();
        }

        private void Destroy() => UnityEngine.Object.Destroy(this);

        private void LogPause()
        {
            if (this.DiagnosticLogging)
                this.taskRunner.ForAllRunningTasks(t => t.DiagTracer.LogPaused());
        }

        private void LogResume()
        {
            if (this.DiagnosticLogging)
                this.taskRunner.ForAllRunningTasks(t => t.DiagTracer.LogResumed());
        }

        void IExceptionHandler.Handle(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            UnityHelper.LogException(exception, ComponentToFollow ?? this);
        }

        void IDiagnosticLogger.Log(string message)
        {
            Debug.Assert(!string.IsNullOrEmpty(message), "Invalid message");

            UnityEngine.Debug.Log($"[{GetHeader()}] {message}");

            string GetHeader() => !this ? "<destroyed-runner>" : this.gameObject.name;
        }
    }
}
