using System;
using System.Threading;
using System.Threading.Tasks;
using ComponentTask.Exceptions;
using ComponentTask.Tests.EditMode.Exceptions;
using ComponentTask.Tests.EditMode.Helpers;
using ComponentTask.Tests.EditMode.Mocks;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode
{
    public sealed class LocalTaskRunner_MiscTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TraceAsserter.Register();
        }

        [Test]
        public void FinishedTasksAreRemovedFromRunner()
        {
            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                runner.AssertRunningTaskCount(0);
                runner.StartTask(TestOfTAsync);
                runner.AssertRunningTaskCount(1);
                runner.Execute();
                runner.AssertRunningTaskCount(0);

                runner.AssertRunningTaskCount(0);
                runner.StartTask(TestAsync);
                runner.AssertRunningTaskCount(1);
                runner.Execute();
                runner.AssertRunningTaskCount(0);
            }

            async Task TestAsync()
            {
                await Task.Yield();
            }

            async Task<int> TestOfTAsync()
            {
                await Task.Yield();
                return 42;
            }
        }

        [Test]
        public void CanBeDisposedMultipleTimes()
        {
            var exHandler = new MockExceptionHandler();
            var runner = new LocalTaskRunner(exHandler);
            runner.Dispose();
            runner.Dispose();
        }

        [Test]
        public void ThrowsForTaskCreatorsThatReturnsNull()
        {
            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                Assert.Throws<TaskCreatorReturnedNullException>(() => runner.StartTask(TaskReturnNull));
                Assert.Throws<TaskCreatorReturnedNullException>(() => runner.StartTask(TaskOfTReturnNull));
            }

            Task TaskReturnNull() => null;
            Task<int> TaskOfTReturnNull() => null;
        }

        [Test]
        public void DoesNotAllowStartingTaskAfterDispose()
        {
            var exHandler = new MockExceptionHandler();
            var runner = new LocalTaskRunner(exHandler);
            runner.Dispose();

            Assert.Throws<ObjectDisposedException>(() => runner.StartTask(() => Task.CompletedTask));
            Assert.Throws<ObjectDisposedException>(() => runner.StartTask(() => Task.FromResult<int>(42)));
        }

        [Test]
        public void DoesNotAllowExecuteAfterDispose()
        {
            var exHandler = new MockExceptionHandler();
            var runner = new LocalTaskRunner(exHandler);
            runner.Dispose();

            Assert.Throws<ObjectDisposedException>(() => runner.Execute());
        }

        [Test]
        public void ResetsPreviousSynchronizationContextAfterExecute()
        {
            // Set custom sync-context.
            var syncContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                runner.Execute();
            }

            // Verify that its again active.
            Assert.True(SynchronizationContext.Current == syncContext);
        }

        [Test]
        public void ResetsPreviousSynchronizationContextWhenTaskCreatorThrows()
        {
            // Set custom sync-context.
            var syncContext = new MockSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                Assert.Throws<TestException>(() => runner.StartTask(() => throw new TestException()));
            }

            // Verify that its again active.
            Assert.True(SynchronizationContext.Current == syncContext);
        }

        [Test]
        public void StartingCompletedTaskIsReturnedDirectly()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(42);
            Task<int> completedTaskOfInt = tcs.Task;
            Task completedTask = tcs.Task;

            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                var taskOfInt = runner.StartTask(taskCreator: () => completedTaskOfInt);
                Assert.True(object.ReferenceEquals(completedTaskOfInt, taskOfInt));

                var task = runner.StartTask(taskCreator: () => completedTask);
                Assert.True(object.ReferenceEquals(completedTask, task));
            }
        }

        [Test]
        public void StartingCanceledTaskIsReturnedDirectlyAndErrorIsReported()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            Task<int> canceledTaskOfInt = tcs.Task;
            Task canceledTask = tcs.Task;

            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                var taskOfInt = runner.StartTask(taskCreator: () => canceledTaskOfInt);
                Assert.True(object.ReferenceEquals(canceledTaskOfInt, taskOfInt));
                exHandler.AssertPop<ComponentTaskCanceledException>();

                var task = runner.StartTask(taskCreator: () => canceledTask);
                Assert.True(object.ReferenceEquals(canceledTask, task));
                exHandler.AssertPop<ComponentTaskCanceledException>();
            }
        }

        [Test]
        public void StartingFaultedTaskIsReturnedDirectlyAndErrorIsReported()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetException(new TestException());
            Task<int> faultedTaskOfInt = tcs.Task;
            Task faultedTask = tcs.Task;

            var exHandler = new MockExceptionHandler();
            using (var runner = new LocalTaskRunner(exHandler))
            {
                var taskOfInt = runner.StartTask(taskCreator: () => faultedTaskOfInt);
                Assert.True(object.ReferenceEquals(faultedTaskOfInt, taskOfInt));
                exHandler.AssertPop<TestException>();

                var task = runner.StartTask(taskCreator: () => faultedTask);
                Assert.True(object.ReferenceEquals(faultedTask, task));
                exHandler.AssertPop<TestException>();
            }
        }
    }
}
