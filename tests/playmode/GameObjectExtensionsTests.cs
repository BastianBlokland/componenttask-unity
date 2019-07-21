using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Exceptions;
using ComponentTask.Tests.PlayMode.Exceptions;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode
{
    public sealed class GameObjectExtensionsTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator TaskRunningOnGameObjectIsTickedEachFrame()
        {
            const int frames = 10;

            var count = 0;
            var go = new GameObject("TestGameObject");
            go.CreateTaskRunner().StartTask(IncrementCountAsync);

            for (int i = 0; i < frames; i++)
            {
                Assert.AreEqual(i, count);
                yield return null;
            }

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
        public IEnumerator TaskStopsWhenGameObjectIsDestroyed()
        {
            var count = 0;
            var go = new GameObject("TestGameObject");
            go.CreateTaskRunner().StartTask(IncrementCountAsync);

            // Assert that count is increment to 1.
            yield return null;
            Assert.AreEqual(1, count);

            // Destroy game-object.
            Object.Destroy(go);

            // Assert count stays at 1.
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(1, count);
                yield return null;
            }

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
        public IEnumerator ExceptionsThrownInTaskAreLogged()
        {
            var go = new GameObject("TestGameObject");

            TestException.ExpectLog();
            go.CreateTaskRunner().StartTask(Test);

            yield return null;
            Object.Destroy(go);

#pragma warning disable CS1998
            async Task Test()
            {
                throw new TestException();
            }
#pragma warning restore CS1998
        }

        [UnityTest]
        public IEnumerator CreateThrowsWhenCalledFromNonUnityThread()
        {
            var go = new GameObject("TestGameObject");

            Task.Run(() =>
            {
                Assert.Throws<NonUnityThreadException>(() => go.CreateTaskRunner());
            });

            yield return null;
            Object.Destroy(go);
        }
    }
}
