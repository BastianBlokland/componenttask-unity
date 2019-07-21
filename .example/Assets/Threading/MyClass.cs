using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Threading
{
    class MyClass : MonoBehaviour
    {
        void Start()
        {
            this.StartTask(RunAsync);
        }

        async Task RunAsync()
        {
            var result = await Task.Run(VeryExpensiveBlockingCode);
            Debug.Log($"Got value: '{result}'");
        }

        int VeryExpensiveBlockingCode()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return 42;
        }
    }
}
