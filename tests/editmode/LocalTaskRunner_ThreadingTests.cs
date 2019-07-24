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
                for (int i = 0; i < 100; i++)
                    runner.StartTask(StressAsync);

                // Execute enough times to finish all tasks.
                for (int i = 0; i < 999; i++)
                {
                    runner.Execute();
                    Thread.Yield();
                }

                // Assert that all tasks have finished.
                runner.AssertRunningTaskCount(0);
            }

            async Task StressAsync()
            {
                /* This stresses the sync-context by posting callbacks from many different threads. */
                for (int i = 0; i < 100; i++)
                    await Task.Run(() => Thread.Yield());
            }
        }
    }
}
