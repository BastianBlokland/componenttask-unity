using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Tests.PlayMode.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ComponentTask.Tests.PlayMode.Integration
{
    public sealed class StressTests
    {
        private sealed class TestClass : MonoBehaviour
        {
            private const int TasksToRun = 100;

            private int tasksRemaining = TasksToRun;
            private int tasksFinished;

            public bool AllFinished => this.tasksRemaining == 0 && this.tasksFinished == TasksToRun;

            private void Update()
            {
                if (tasksRemaining > 0)
                {
                    this.StartTask(this.RunAsync);
                    tasksRemaining--;
                }
            }

            private async Task RunAsync()
            {
                await Task.Yield();
                await this.StartTask(this.DoWorkAsync);
                await Task.Yield();
                this.tasksFinished++;
            }

            private async Task DoWorkAsync()
            {
                await Task.Yield();
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [UnityTest]
        public IEnumerator AllTasksFinishStressTest()
        {
            var go = new GameObject("Test");
            var testClass = go.AddComponent<TestClass>();

            for (int i = 0; i < 500; i++)
            {
                if (testClass.AllFinished)
                    break;

                yield return null;
            }

            Assert.True(testClass.AllFinished, "Work did not finish");

            // Cleanup.
            UnityEngine.Object.Destroy(go);
        }
    }
}
