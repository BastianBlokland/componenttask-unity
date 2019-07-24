using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class DisableGameObjectAtEnd
    {
        private sealed class ClassA : MonoBehaviour
        {
            public ClassB Worker { get; set; }

            public bool IsFinished { get; private set; }

            private void Start()
            {
                this.StartTask(this.RunAsync);
            }

            private async Task RunAsync()
            {
                await Task.Yield();

                await this.Worker.RunAsync();
                await this.Worker.RunAsync();

                await Task.Yield();
                this.IsFinished = true;
            }
        }

        private sealed class ClassB : MonoBehaviour
        {
            public Task RunAsync()
            {
                this.gameObject.SetActive(true);
                return this.StartTask(this.RunImplAsync);
            }

            private async Task RunImplAsync()
            {
                await Task.Yield();
                await Task.Yield();
                this.gameObject.SetActive(false);
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator TaskCompletesWhenGameObjectIsDisabledAtEnd()
        {
            var goB = new GameObject("B");
            var classB = goB.AddComponent<ClassB>();
            goB.SetActive(false);

            var goA = new GameObject("A");
            var classA = goA.AddComponent<ClassA>();
            classA.Worker = classB;

            for (int i = 0; i < 25; i++)
            {
                if (classA.IsFinished)
                    break;

                yield return null;
            }

            Assert.True(classA.IsFinished, "Work did not finish");

            // Cleanup.
            UnityEngine.Object.Destroy(goA);
            UnityEngine.Object.Destroy(goB);
        }
    }
}
