using System;
using System.Collections;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class WrappingTasks
    {
        private sealed class ClassA : MonoBehaviour
        {
            public TaskWrapper Wrapper { get; set; }

            public bool IsFinished { get; set; }

            private void Start()
            {
                this.StartTask(this.RunAsync);
            }

            private async Task RunAsync()
            {
                ComponentContextAsserter.AssertRunningInComponentContext(this);

                await Task.Yield();
                for (int i = 0; i < 3; i++)
                {
                    await this.Wrapper.WrapWork(() => this.StartTask(this.DoWorkAsync));
                    ComponentContextAsserter.AssertRunningInComponentContext(this);
                }

                this.IsFinished = true;
            }

            private async Task DoWorkAsync()
            {
                ComponentContextAsserter.AssertRunningInComponentContext(this);
                await Task.Yield();
                ComponentContextAsserter.AssertRunningInComponentContext(this);
                await Task.Yield();
                ComponentContextAsserter.AssertRunningInComponentContext(this);
            }
        }

        private sealed class TaskWrapper : MonoBehaviour
        {
            public Task WrapWork(Func<Task> taskCreator)
            {
                return this.StartTask(RunAsync);

                async Task RunAsync()
                {
                    ComponentContextAsserter.AssertRunningInComponentContext(this);

                    // Do something before.
                    await Task.Yield();

                    ComponentContextAsserter.AssertRunningInComponentContext(this);

                    // Execute work.
                    await taskCreator.Invoke();

                    ComponentContextAsserter.AssertRunningInComponentContext(this);

                    // Do something after.
                    await Task.Yield();

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
        public IEnumerator SyncContextIsFlowedCorrectlyWhenWrappingTasks()
        {
            var goB = new GameObject("B");
            var classB = goB.AddComponent<TaskWrapper>();

            var goA = new GameObject("A");
            var classA = goA.AddComponent<ClassA>();
            classA.Wrapper = classB;

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
