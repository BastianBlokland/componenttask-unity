using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AwaitingMethods
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
                var val = await GetValueAsync();
                Debug.Log($"Got value: '{val}'");
            }
        }

        async Task<int> GetValueAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return Time.frameCount;
        }
    }
}
