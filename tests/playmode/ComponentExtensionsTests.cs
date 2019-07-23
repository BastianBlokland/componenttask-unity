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
            var t = comp.StartTask(IncrementCountAsync);

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
        public IEnumerator MultipleTasksCanRunInParallelOnSameComponent()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(IncrementCountAsync);
            comp.StartTask(IncrementCountAsync);
            comp.StartTask(IncrementCountAsync);

            yield return null;
            Assert.AreEqual(3, count);

            yield return null;
            Assert.AreEqual(6, count);

            yield return null;
            Assert.AreEqual(9, count);

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync()
            {
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
            }
        }

        [UnityTest]
        public IEnumerator MultipleTasksCanRunInSequenceOnSameComponent()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(IncrementCountAsync, 2);

            // Assert count is increased every frame for 9 frames.
            for (int i = 0; i < 9; i++)
            {
                Assert.AreEqual(i, count);
                yield return null;
            }

            // Assert count stays at 9.
            yield return null;
            Assert.AreEqual(9, count);

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync(int iters)
            {
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;

                if (iters > 0)
                    comp.StartTask(IncrementCountAsync, iters - 1).DontWait();
            }
        }

        [UnityTest]
        public IEnumerator MultipleTasksCanRunInSequenceWithPausesOnSameComponent()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();

            // Run tasks.
            comp.StartTask(IncrementCountAsync);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, count);
                yield return null;
            }

            // Idle for couple of frames.
            yield return null;
            yield return null;

            // Run another task.
            comp.StartTask(IncrementCountAsync);
            for (int i = 3; i < 6; i++)
            {
                Assert.AreEqual(i, count);
                yield return null;
            }

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync()
            {
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
            }
        }

        [UnityTest]
        public IEnumerator TaskPausesWhenComponentIsDisabled()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(IncrementCountAsync);

            // Assert task is running.
            yield return null;
            Assert.AreEqual(1, count);

            // Disable component.
            comp.enabled = false;

            // Assert task is paused.
            yield return null;
            Assert.AreEqual(1, count);
            yield return null;
            Assert.AreEqual(1, count);

            // Enable component.
            comp.enabled = true;

            // Assert task is running.
            yield return null;
            Assert.AreEqual(2, count);
            yield return null;
            Assert.AreEqual(3, count);

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync()
            {
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
            }
        }

        [UnityTest]
        public IEnumerator TaskRunsWhileComponentIsDisabledWhenConfigured()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(IncrementCountAsync, TaskRunOptions.UpdateWhileComponentDisabled);

            // Assert task is running.
            yield return null;
            Assert.AreEqual(1, count);

            // Disable component.
            comp.enabled = false;

            // Assert task is still running.
            yield return null;
            Assert.AreEqual(2, count);
            yield return null;
            Assert.AreEqual(3, count);

            // Cleanup.
            Object.Destroy(go);

            async Task IncrementCountAsync()
            {
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
                await Task.Yield();
                count++;
            }
        }

        [UnityTest]
        public IEnumerator SameRunnerIsReusedForTheSameComponentAndSameOptions()
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
        public IEnumerator DifferentRunnerIsUsedForSameComponentAndDifferentOptions()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();

            var runner1 = comp.GetTaskRunner(TaskRunOptions.Default);
            var runner2 = comp.GetTaskRunner(TaskRunOptions.UpdateWhileComponentDisabled);
            Assert.False(object.ReferenceEquals(runner1, runner2));

            yield return null;
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator RunnerDestroysItselfWhenComponentIsDestroyed()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(TestAsync);

            // Assert that runner was created.
            Assert.AreEqual(2, go.GetComponents<MonoBehaviour>().Length);

            // Destroy component.
            Object.DestroyImmediate(comp);
            yield return null;

            // Assert that runner has destroyed itself.
            Assert.AreEqual(0, go.GetComponents<MonoBehaviour>().Length);

            // Cleanup.
            Object.Destroy(go);

            async Task TestAsync()
            {
                await Task.Yield();
                await Task.Yield();
            }
        }

        [UnityTest]
        public IEnumerator RunnerDestroysItselfWhenDisabledComponentIsDestroyed()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(TestAsync);

            // Disable component.
            comp.enabled = false;

            // Assert that runner was created.
            Assert.AreEqual(2, go.GetComponents<MonoBehaviour>().Length);

            // Destroy component.
            Object.DestroyImmediate(comp);
            yield return null;

            // Assert that runner has destroyed itself.
            Assert.AreEqual(0, go.GetComponents<MonoBehaviour>().Length);

            // Cleanup.
            Object.Destroy(go);

            async Task TestAsync()
            {
                await Task.Yield();
                await Task.Yield();
            }
        }

        [UnityTest]
        public IEnumerator RunnerDestroysItselfWhenWorkIsDone()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            comp.StartTask(TestAsync);

            // Assert that runner was created.
            Assert.AreEqual(2, go.GetComponents<MonoBehaviour>().Length);

            // Wait for work to finish.
            yield return null;

            // Assert that runner has destroyed itself.
            Assert.AreEqual(1, go.GetComponents<MonoBehaviour>().Length);

            // Cleanup.
            yield return null;
            Object.Destroy(go);

            async Task TestAsync()
            {
                await Task.Yield();
            }
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

        [UnityTest]
        public IEnumerator NullComponentThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => ComponentExtensions.GetTaskRunner(null));
            Assert.Throws<System.ArgumentNullException>(() => ComponentExtensions.StartTask(null, () => Task.CompletedTask));
            yield break;
        }

        [UnityTest]
        public IEnumerator DestroyedComponentThrowsMissingReferenceException()
        {
            var go = new GameObject("TestGameObject");
            var comp = go.AddComponent<MockComponent>();
            Object.DestroyImmediate(comp);

            Assert.Throws<MissingReferenceException>(() => ComponentExtensions.GetTaskRunner(comp));
            Assert.Throws<MissingReferenceException>(() => ComponentExtensions.StartTask(comp, () => Task.CompletedTask));

            yield return null;
            Object.Destroy(go);
        }
    }
}
