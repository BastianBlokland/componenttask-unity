using System;
using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

namespace CustomLocalTaskRunner
{
    class MyClass : MonoBehaviour, IExceptionHandler
    {
        [SerializeField] private bool isPaused;

        private LocalTaskRunner runner;

        void Start()
        {
            this.runner = new LocalTaskRunner(exceptionHandler: this);
            this.runner.StartTask(this.RunAsync);
        }

        void Update()
        {
            if (!this.isPaused)
                this.runner.Execute();
        }

        void OnDestroy()
        {
            this.runner.Dispose();
        }

        async Task RunAsync()
        {
            while (true)
            {
                Debug.Log("Running");
                await Task.Yield();
            }
        }

        void IExceptionHandler.Handle(Exception exception)
        {
            Debug.Log($"Exception occurred: '{exception.Message}'");
        }
    }
}
