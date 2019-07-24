using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class ProducerConsumerTests
    {
        private sealed class Producer : MonoBehaviour
        {
            public Task<int> GetValueAsync(string id) => this.StartTask(this.GetValueAsyncImpl, id);

            async Task<int> GetValueAsyncImpl(string id)
            {
                await Task.Yield();
                await Task.Yield();
                return 42;
            }
        }

        private sealed class Consumer : MonoBehaviour
        {
            public int CurrentValue;
            public Producer ValueProducer;

            void Start()
            {
                this.StartTask(RunAsync);
            }

            async Task RunAsync()
            {
                try
                {
                    this.CurrentValue = await this.ValueProducer.GetValueAsync(id: "1337");
                }
                catch (TaskCanceledException)
                {
                    // Producer got destroyed before it could produce the result.
                    this.CurrentValue = -1;
                }
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator ConsumerReceivesValueFromConsumer()
        {
            var producerGo = new GameObject("Producer");
            var producer = producerGo.AddComponent<Producer>();

            var consumerGo = new GameObject("Consumer");
            var consumer = consumerGo.AddComponent<Consumer>();
            consumer.ValueProducer = producer;

            // Wait 1 frame for the 'Start' method of consumer.
            yield return null;

            // Wait two frames for the producer work to be done.
            yield return null;
            yield return null;

            // Wait 1 frame for consumer to observe.
            yield return null;

            Assert.AreEqual(42, consumer.CurrentValue);

            // Cleanup.
            Object.Destroy(producerGo);
            Object.Destroy(consumerGo);
        }

        [UnityTest]
        public IEnumerator ConsumerObservesTheProducerBeingDestroyed()
        {
            var producerGo = new GameObject("Producer");
            var producer = producerGo.AddComponent<Producer>();

            var consumerGo = new GameObject("Consumer");
            var consumer = consumerGo.AddComponent<Consumer>();
            consumer.ValueProducer = producer;

            // Wait 1 frame for the 'Start' method of consumer.
            yield return null;

            // Destroy the producer.
            Object.DestroyImmediate(producerGo);

            // Wait 1 frame for consumer to observe.
            yield return null;

            Assert.AreEqual(-1, consumer.CurrentValue);

            // Cleanup.
            Object.Destroy(consumerGo);
        }
    }
}
