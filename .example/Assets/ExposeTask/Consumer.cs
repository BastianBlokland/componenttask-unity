using System.Threading.Tasks;
using UnityEngine;

namespace ExposeTask
{
    class Consumer : MonoBehaviour
    {
        [SerializeField] private Producer producer;

        void Start()
        {
            this.StartTask(RunAsync);
        }

        async Task RunAsync()
        {
            try
            {
                var val = await producer.GetValueAsync();
                Debug.Log($"Got value: '{val}'");
            }
            catch (TaskCanceledException)
            {
                Debug.Log("The producer was destroyed before producing the result");
            }
        }
    }
}
