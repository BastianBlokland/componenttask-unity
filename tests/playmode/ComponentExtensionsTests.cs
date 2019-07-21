using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Exceptions;
using ComponentTask.Tests.PlayMode.Helpers;
using ComponentTask.Tests.PlayMode.Mocks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode
{
    public sealed class ComponentExtensionsTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator TaskStopsWhenComponentIsDestroyed()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            var t = comp.GetTaskRunner().StartTask(IncrementCountAsync);

            // Assert that count is increment to 1.
            yield return null;
            Assert.AreEqual(1, count);

            // Destroy the component.
            Object.Destroy(comp);

            // Assert count stays at 1.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(1, count);
                yield return null;
            }

            Assert.True(t.IsCanceled, "Task was not cancelled when component was destroyed");

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync()
            {
                while (true)
                {
                    await Task.Yield();
                    count++;
                }
            }
        }

        [UnityTest]
        public IEnumerator SameRunnerIsReusedForTheSameComponent()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();

            var runner1 = comp.GetTaskRunner();
            var runner2 = comp.GetTaskRunner();
            Assert.True(object.ReferenceEquals(runner1, runner2));

            yield return null;
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledFromNonUnityThread()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();

            Task.Run(() =>
            {
                Assert.Throws<NonUnityThreadException>(() => comp.GetTaskRunner());
                Assert.Throws<NonUnityThreadException>(() => comp.StartTask(() => Task.CompletedTask));
            });

            yield return null;
            Object.Destroy(go);
        }
    }
}
