using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class BackgroundWorkTests
    {
        private sealed class BackgroundWorker : MonoBehaviour
        {
            public Task<int> BackgroundWork { get; private set; }

            void Awake()
            {
                this.BackgroundWork = this.StartTask(this.BackgroundWorkAsync);
            }

            private async Task<int> BackgroundWorkAsync(CancellationToken cancelToken)
            {
                // Perform heavy computation on a background thread.
                return await Task.Run(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return -1;
                        Thread.Sleep(10);
                    }

                    return 42;
                });
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator ValueFromBackgroundWorkerIsReceived()
        {
            var result = 0;
            var go = new GameObject("BackgroundWork");
            var worker = go.AddComponent<BackgroundWorker>();
            go.CreateTaskRunner().StartTask(WaitForBackgroundWorkAsync);

            // Wait for work to finish.
            yield return new WaitForSeconds(1);

            Assert.AreEqual(42, result);

            // Cleanup.
            Object.Destroy(go);

            async Task WaitForBackgroundWorkAsync()
            {
                result = await worker.BackgroundWork;
            }
        }
    }
}
