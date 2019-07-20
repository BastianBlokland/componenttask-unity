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

        public Task StartTask(Func<Task> taskCreator) =>
            this.taskRunner.StartTask(taskCreator);

        public Task StartTask(Func<CancellationToken, Task> taskCreator) =>
            this.taskRunner.StartTask(taskCreator);

        public Task StartTask<TIn>(Func<TIn, Task> taskCreator, TIn data) =>
            this.taskRunner.StartTask(taskCreator, data);

        public Task StartTask<TIn>(Func<TIn, CancellationToken, Task> taskCreator, TIn data) =>
            this.taskRunner.StartTask(taskCreator, data);

        public Task<TOut> StartTask<TOut>(Func<Task<TOut>> taskCreator) =>
            this.taskRunner.StartTask(taskCreator);

        public Task<TOut> StartTask<TOut>(Func<CancellationToken, Task<TOut>> taskCreator) =>
            this.taskRunner.StartTask(taskCreator);

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, Task<TOut>> taskCreator, TIn data) =>
            this.taskRunner.StartTask(taskCreator, data);

        public Task<TOut> StartTask<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data) =>
            this.taskRunner.StartTask(taskCreator, data);

        // Dynamically called from the Unity runtime.
        private void LateUpdate()
        {
            // Check if we have a 'ComponentToFollow' assigned.
            if (this.ComponentToFollow is null)
            {
                // If not then always just execute the runner.
                this.taskRunner.Execute();
            }
            else
            {
                // If the component we are following has been destroyed then we destroy ourselves.
                if (!this.ComponentToFollow)
                    UnityEngine.Object.Destroy(this);
                else
                {
                    // If the component is a 'Behaviour' then we update when its enabled.
                    if (ComponentToFollow is UnityEngine.Behaviour behaviour)
                    {
                        if (behaviour.isActiveAndEnabled)
                            this.taskRunner.Execute();
                    }
                    else
                    {
                        // Otherwise we always update.
                        this.taskRunner.Execute();
                    }
                }
            }
        }

        // Dynamically called from the Unity runtime.
        private void OnDestroy() => this.taskRunner.Dispose();

        void IExceptionHandler.Handle(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            UnityHelper.LogException(exception, ComponentToFollow ?? this);
        }
    }
}
