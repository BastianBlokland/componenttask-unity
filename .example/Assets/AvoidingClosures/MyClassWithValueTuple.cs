using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AvoidingClosures
{
    class MyClassWithValueTuple : MonoBehaviour
    {
        void Start()
        {
            this.StartTask(WaitAndLog, (secondsDelay: 1, message: "Hello World"));
        }

        async Task WaitAndLog((int secondsDelay, string message) input)
        {
            await Task.Delay(TimeSpan.FromSeconds(input.secondsDelay));
            Debug.Log(input.message);
        }
    }
}
