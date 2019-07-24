using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Tests.EditMode.Helpers;
using ComponentTask.Tests.EditMode.Mocks;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode
{
    public sealed class LocalTaskRunner_ThreadingTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [Test]
        public void CallbackPostThreadingStressTest()
        {
            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                // Start the stress tasks.
                for (int i = 0; i < 50; i++)
                    runner.StartTask(StressAsync);

                // Execute enough times to finish all tasks.
                for (int i = 0; i < 99; i++)
                {
                    runner.Execute();
                    Thread.Sleep(10);
                }

                // Assert that all tasks have finished.
                runner.AssertRunningTaskCount(0);
            }

            async Task StressAsync()
            {
                /* This stresses the sync-context by posting callbacks from many different threads. */
                for (int i = 0; i < 50; i++)
                    await Task.Run(() => Thread.Yield());
            }
        }

        [Test]
        public void ExecuteThreadingStressTest()
        {
            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                // Start many tasks tasks.
                for (int i = 0; i < 100; i++)
                    runner.StartTask(TestAsync);

                for (int interation = 0; interation < 11; interation++)
                {
                    // Execute from many different threads in parallel.
                    for (int thread = 0; thread < 100; thread++)
                        ThreadPool.QueueUserWorkItem(r => ((LocalTaskRunner)r).Execute(), runner);

                    Thread.Sleep(millisecondsTimeout: 100);
                }

                // Assert that all tasks have finished.
                runner.AssertRunningTaskCount(0);
            }

            async Task TestAsync()
            {
                for (int i = 0; i < 10; i++)
                    await Task.Yield();
            }
        }

        [Test]
        public void StartTaskThreadingStressTest()
        {
            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                // Start many tasks tasks in parallel.
                for (int i = 0; i < 100; i++)
                    ThreadPool.QueueUserWorkItem(r => ((LocalTaskRunner)r).StartTask(TestAsync), runner);

                // Wait for tasks to start.
                Thread.Sleep(millisecondsTimeout: 1000);

                // Execute a single tick to finish all tasks.
                runner.Execute();

                // Assert that all tasks have finished.
                runner.AssertRunningTaskCount(0);
            }

            async Task TestAsync()
            {
                await Task.Yield();
            }
        }
    }
}
