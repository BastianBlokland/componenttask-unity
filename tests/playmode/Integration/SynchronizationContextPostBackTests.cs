using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class SynchronizationContextPostBackTests
    {
        private sealed class TestClass : MonoBehaviour
        {
            private readonly ThreadedEvent threadedEvent = new ThreadedEvent();
            private readonly List<object> receivedData = new List<object>();

            public IReadOnlyList<object> ReceivedData => this.receivedData;

            private void Start() => this.StartTask(this.RunAsync);

            private async Task RunAsync()
            {
                ComponentContextAsserter.AssertRunningInComponentContext(this);

                await Task.Yield();

                ComponentContextAsserter.AssertRunningInComponentContext(this);
                this.threadedEvent.Subscribe(this.OnWorkFinished);
                Task.Run(this.BackgroundCalculateAsync).DontWait();
            }

            private void OnWorkFinished(object data)
            {
                ComponentContextAsserter.AssertRunningInComponentContext(this);
                this.receivedData.Add(data);
            }

            private async Task BackgroundCalculateAsync()
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(42)).ConfigureAwait(false);
                    this.threadedEvent.Invoke(i);
                }
            }
        }

        private sealed class ThreadedEvent
        {
            private readonly object listenersLock = new object();
            private readonly List<(Action<object> callback, SynchronizationContext context)> listeners =
                new List<(Action<object>, SynchronizationContext)>();

            public void Subscribe(Action<object> callback)
            {
                var syncContext = SynchronizationContext.Current;
                lock (this.listenersLock)
                {
                    this.listeners.Add((callback, syncContext));
                }
            }

            public void Invoke(object data)
            {
                lock (this.listenersLock)
                {
                    foreach (var listener in this.listeners)
                        listener.context.Post(s => listener.callback(s), data);
                }
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator SynchronizationPostBacksAreDelivered()
        {
            /* This test simulates a pattern where you 'subscribe' to a 'event' but that event
            captures the sync-context and 'invokes' the event there. This is a pretty common pattern
            to handle events in a multi-threaded environment.*/

            var go = new GameObject("TestClass");
            var testClass = go.AddComponent<TestClass>();

            // Wait for the work to finish.
            yield return new WaitForSeconds(1f);

            CollectionAssert.AreEquivalent(new object[] { 0, 1, 2, 3, 4 }, testClass.ReceivedData);

            // Cleanup.
            UnityEngine.Object.Destroy(go);
        }
    }
}
