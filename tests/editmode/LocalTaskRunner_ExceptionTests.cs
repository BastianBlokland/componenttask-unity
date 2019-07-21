using System.Threading.Tasks;
using ComponentTask.Tests.EditMode.Exceptions;
using ComponentTask.Tests.EditMode.Helpers;
using ComponentTask.Tests.EditMode.Mocks;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode.ComponentTask
{
    public sealed class LocalTaskRunner_ExceptionTests
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
        public void ExceptionsInSynchronousMethodAreThrownDirectly()
        {
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                Assert.Throws<TestException>(() => runner.StartTask(TestAsync));
            }

            Task TestAsync()
            {
                throw new TestException();
            }
        }

        [Test]
        public void ExceptionsAreReportedInSynchronousPartOfAsyncMethod()
        {
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(TestAsync);
                this.exceptionHandler.AssertPop<TestException>();
            }

#pragma warning disable CS1998
            async Task TestAsync()
            {
                throw new TestException();
            }
#pragma warning restore CS1998
        }

        [Test]
        public void ExceptionsAreReportedInAsyncMethod()
        {
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(TestAsync);
                this.exceptionHandler.AssertNoExceptions();
                runner.Execute();
                this.exceptionHandler.AssertPop<TestException>();
            }

            async Task TestAsync()
            {
                await Task.Yield();
                throw new TestException();
            }
        }

        [Test]
        public void ExceptionsOfCalledMethodsAreReported()
        {
            using (var runner = new LocalTaskRunner(this.exceptionHandler))
            {
                runner.StartTask(Test1Async);
                this.exceptionHandler.AssertNoExceptions();
                runner.Execute();
                this.exceptionHandler.AssertPop<TestException>();
            }

            async Task Test1Async()
            {
                await Test2Async();
            }

            async Task Test2Async()
            {
                await Task.Yield();
                throw new TestException();
            }
        }

        [Test]
        public void CancelledExceptionIsReportedWhenAwaitingDisposedRunner()
        {
            using (var runner1 = new LocalTaskRunner(this.exceptionHandler))
            {
                using (var runner2 = new LocalTaskRunner(this.exceptionHandler))
                {
                    runner1.StartTask(TestAsync, runner2);
                    this.exceptionHandler.AssertNoExceptions();
                }

                runner1.Execute();
                this.exceptionHandler.AssertPop<TaskCanceledException>();
            }

            async Task TestAsync(ITaskRunner r)
            {
                await r.StartTask(GetIntAsync);
            }

            async Task<int> GetIntAsync()
            {
                await Task.Yield();
                return 42;
            }
        }

        [Test]
        public void CancelledTaskExceptionCanBeCaught()
        {
            using (var runner1 = new LocalTaskRunner(this.exceptionHandler))
            {
                Task<int> t;
                using (var runner2 = new LocalTaskRunner(this.exceptionHandler))
                    t = runner1.StartTask(TestAsync, runner2);

                runner1.Execute();
                Assert.AreEqual(-1, t.Result);
            }

            async Task<int> TestAsync(ITaskRunner r)
            {
                try
                {
                    return await r.StartTask(GetIntAsync);
                }
                catch (TaskCanceledException)
                {
                    return -1;
                }
            }

            async Task<int> GetIntAsync()
            {
                await Task.Yield();
                return 42;
            }
        }
    }
}
