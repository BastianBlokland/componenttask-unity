using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ExposeTask
{
    class Producer : MonoBehaviour
    {
        public Task<int> GetValueAsync() => this.StartTask(ProduceValueAsync);

        async Task<int> ProduceValueAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return Time.frameCount;
        }
    }
}
