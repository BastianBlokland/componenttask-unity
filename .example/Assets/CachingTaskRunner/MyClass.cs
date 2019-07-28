using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

namespace CachingTaskRunner
{
    class MyClass : MonoBehaviour
    {
        private ITaskRunner runner;

        void Start()
        {
            this.runner = this.gameObject.CreateTaskRunner();
        }

        void Update()
        {
            this.runner.StartTask(this.WaitAndLogAsync);
        }

        async Task WaitAndLogAsync()
        {
            await Task.Yield();
            Debug.Log("Running");
        }
    }
}
