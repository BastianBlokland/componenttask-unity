using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AvoidingClosures
{
    class MyClass : MonoBehaviour
    {
        void Start()
        {
            var delay = 1;
            this.StartTask(WaitAndDoSomethingAsync, delay);
        }

        async Task WaitAndDoSomethingAsync(int secondsDelay)
        {
            await Task.Delay(TimeSpan.FromSeconds(secondsDelay));
            Debug.Log("Doing something");
        }
    }
}
