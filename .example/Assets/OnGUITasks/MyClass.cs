using System;
using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

namespace OnGUITasks
{
    class MyClass : MonoBehaviour, IExceptionHandler
    {
        private LocalTaskRunner guiTaskRunner;

        void Start()
        {
            this.guiTaskRunner = new LocalTaskRunner(exceptionHandler: this);
            this.guiTaskRunner.StartTask(this.DrawUIAsync);
        }

        void OnGUI()
        {
            this.guiTaskRunner.Execute();
        }

        void OnDestroy()
        {
            this.guiTaskRunner.Dispose();
        }

        async Task DrawUIAsync()
        {
            while (true)
            {
                await Task.Yield();
                GUI.Label(new Rect(0f, 0f, 100f, 100f), "Drawn from a task :)");
            }
        }

        void IExceptionHandler.Handle(Exception exception)
        {
            Debug.Log($"Exception occurred: '{exception.Message}'");
        }
    }
}
