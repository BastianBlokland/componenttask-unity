using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComponentTask.Internal
{
    internal sealed class MonoBehaviourTaskRunner : UnityEngine.MonoBehaviour, ITaskRunner, IExceptionHandler
    {
        private readonly LocalTaskRunner taskRunner;

        public MonoBehaviourTaskRunner()
        {
            this.taskRunner = new LocalTaskRunner(exceptionHandler: this);
        }

        public UnityEngine.Component ComponentToFollow { get; set; }

        public bool IsFinished { get; private set; }

        public Task StartTask(Func<Task> taskCreator)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator);
        }

        public Task StartTask(Func<CancellationToken, Task> taskCreator)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator);
        }

        public Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator, data);
        }

        public Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator, data);
        }

        public Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator);
        }

        public Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator);
        }

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator, data);
        }

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data)
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Task was started on already finished runner");
            return this.taskRunner.StartTask(taskCreator, data);
        }

        // Dynamically called from the Unity runtime.
        private void LateUpdate()
        {
            System.Diagnostics.Debug.Assert(!this.IsFinished, "Already finished runner was updated");

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
                        if (behaviour.isActiveAndEnabled)
                            this.Execute();
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
            var workRemaining = this.taskRunner.Execute();

            // If we've finished all the work then destroy ourselves.
            if (!workRemaining)
                this.Destroy();
        }

        private void Destroy()
        {
            this.IsFinished = true;
            UnityEngine.Object.Destroy(this);
        }

        void IExceptionHandler.Handle(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            UnityHelper.LogException(exception, ComponentToFollow ?? this);
        }
    }
}
