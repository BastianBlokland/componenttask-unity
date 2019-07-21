using System;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Tests.EditMode.Exceptions;
using ComponentTask.Tests.EditMode.Helpers;
using ComponentTask.Tests.EditMode.Mocks;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode
{
    public sealed class LocalTaskRunner_SchedulingTests
    {
        private MockExceptionHandler exceptionHandler;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [SetUp]
        public void Setup()
        {
            this.exceptionHandler = new MockExceptionHandler();
        }

        [TearDown]
        public void TearDown()
        {
            this.exceptionHandler.AssertNoExceptions();
        }

        [Test]
        public void CodeBeforeAwaitRunsSynchronously()
        {
            var i = 0;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(IncrementAndWaitAsync);
                Assert.AreEqual(1, i);
            }

            async Task IncrementAndWaitAsync()
            {
                i++;
                await Task.Yield();
            }
        }

        [Test]
        public void YieldWaitsForNextExecute()
        {
            const int iterations = 25;

            var count = 0;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(WaitAndIncrementAsync);
                for (int i = 0; i < iterations; i++)
                {
                    Assert.AreEqual(i, count);
                    runner.Execute();
                }
            }

            async Task WaitAndIncrementAsync()
            {
                for (int i = 0; i < iterations; i++)
                {
                    await Task.Yield();
                    count++;
                }
            }
        }

        [Test]
        public void AwaitingCompletedTaskExecutesSynchronously()
        {
            var count = 0;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(TestAsync);
                Assert.AreEqual(1, count);
            }

            async Task TestAsync()
            {
                await Task.CompletedTask;
                await Task.CompletedTask;
                count++;
            }
        }

        [Test]
        public void AwaitingSynchronousDependencyExecutesSynchronously()
        {
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                var t = runner.StartTask(TestAsync, runner);
                t.AssertCompletedSuccessfully();
                Assert.AreEqual(2, t.Result);
            }

            async Task<int> TestAsync(ITaskRunner runner)
            {
                if (await runner.StartTask(DependencyAsync))
                    return 1;
                return 2;
            }

            async Task<bool> DependencyAsync()
            {
                if (DateTime.UtcNow.Year > 0)
                    return false;

                await Task.Delay(TimeSpan.FromHours(1));
                return true;
            }
        }

        [Test]
        public void RunnerScopePropagatesToCalledMethods()
        {
            var count = 0;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                var t = runner.StartTask(TestAsync);
                runner.Execute();
                Assert.AreEqual(0, count);
                runner.Execute();
                Assert.AreEqual(1, count);
                runner.Execute();
                Assert.AreEqual(1, count);
                runner.Execute();
                Assert.AreEqual(2, count);
                t.AssertCompletedSuccessfully();
            }

            async Task TestAsync()
            {
                count += await SubTestAsync();
                count += await SubTestAsync();
            }

            async Task<int> SubTestAsync()
            {
                await Task.Yield();
                await Task.Yield();
                return 1;
            }
        }

        [Test]
        public void TaskGetsCancelledWhenRunnerIsDisposed()
        {
            Task t;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                t = runner.StartTask(TestAsync);
                t.AssertRunning();
            }
            t.AssertCanceled();

            async Task TestAsync()
            {
                while (true)
                {
                    await Task.Yield();
                }
            }
        }

        [Test]
        public void TaskIsResumedWhenDependencyIsDone()
        {
            var count = 0;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            using (var dependencyRunner = new LocalTaskRunner(this.exceptionHandler))
            {
                var dep = dependencyRunner.StartTask(DependencyAsync);
                var t = runner.StartTask(TestAsync, dep);

                // Task does not complete on its own.
                runner.Execute();
                Assert.AreEqual(0, count);

                // But when completing dependency.
                dependencyRunner.Execute();
                Assert.AreEqual(0, count);

                // And then executing the original running the task is completed.
                runner.Execute();
                t.AssertCompletedSuccessfully();
                Assert.AreEqual(1, count);
            }

            async Task TestAsync(Task dependency)
            {
                await dependency;
                count++;
            }

            async Task DependencyAsync()
            {
                await Task.Yield();
            }
        }

        [Test]
        public void CancelTokenIsTrippedWhenDisposingRunning()
        {
            var heavyCalcRunning = false;
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(TestAsync);

                // Wait for 'heavyCalc' to start on a different thread.
                while (!Volatile.Read(ref heavyCalcRunning))
                    Thread.Yield();
            }

            // Assert that 'HeavyCalc' stops.
            for (int i = 0; i < 100_000; i++)
            {
                if (!Volatile.Read(ref heavyCalcRunning))
                    return;
                Thread.Yield();
            }

            Assert.Fail("'HeavyCalcAsync' did not stop");

            async Task TestAsync(CancellationToken cancelToken)
            {
                await Task.Run(() => HeavyCalcAsync(cancelToken));
            }

            async Task<int> HeavyCalcAsync(CancellationToken cancelToken)
            {
                heavyCalcRunning = true;
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), cancelToken);
                    return 1;
                }
                catch (OperationCanceledException)
                {
                    heavyCalcRunning = false;
                    return -1;
                }
            }
        }
    }
}
