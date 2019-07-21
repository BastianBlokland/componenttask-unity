using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cancelling
{
    class MyClass : MonoBehaviour
    {
        void Start()
        {
            var url = "https://github.com/BastianBlokland/componenttask-unity";
            this.StartTask(DownloadTextAsync, url);
        }

        async Task DownloadTextAsync(string url, CancellationToken cancelToken)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url, cancelToken);
                var responseText = await response.Content.ReadAsStringAsync();
                Debug.Log($"Text: '{responseText}'");
            }
        }
    }
}
