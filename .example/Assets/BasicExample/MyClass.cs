using System.Threading.Tasks;
using UnityEngine;

namespace BasicExample
{
    class MyClass : MonoBehaviour
    {
        void Start()
        {
            this.StartTask(RunAsync);
        }

        async Task RunAsync()
        {
            while (true)
            {
                Debug.Log("Running...");
                await Task.Yield();
            }
        }
    }
}
