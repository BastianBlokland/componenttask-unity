using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class InactiveParentTests
    {
        private sealed class ParentClass : MonoBehaviour
        {
            public ChildClass Child { get; set; }

            private void Start()
            {
                this.StartTask(this.TaskRunAsync);
            }

            private async Task TaskRunAsync()
            {
                ComponentContextAsserter.AssertRunningInComponentContext(this);

                /* Disable ourself and start a child task. */

                this.gameObject.SetActive(false);
                await this.Child.DoWork();
            }
        }

        private sealed class ChildClass : MonoBehaviour
        {
            public bool WorkFinished { get; private set; }

            public Task DoWork()
            {
                return this.StartTask(RunAsync);

                async Task RunAsync()
                {
                    ComponentContextAsserter.AssertRunningInComponentContext(this);

                    await Task.Yield();
                    this.WorkFinished = true;

                    ComponentContextAsserter.AssertRunningInComponentContext(this);
                }
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator ChildTaskFinishesEvenIfParentIsInactive()
        {
            var parentGo = new GameObject("parent");
            var parentClass = parentGo.AddComponent<ParentClass>();

            var childGo = new GameObject("child");
            var childClass = childGo.AddComponent<ChildClass>();
            parentClass.Child = childClass;

            // Wait for child task to start and finish.
            yield return null;
            yield return null;

            Assert.True(childClass.WorkFinished, "Child did not finish work");

            // Cleanup.
            UnityEngine.Object.Destroy(parentGo);
            UnityEngine.Object.Destroy(childClass);
        }
    }
}
