using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode.Helpers
{
    public static class TaskExtensions
    {
        public static void AssertRunning(this Task task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));
            Assert.False(task.IsCompleted, "Expected task to be running but it was completed.");
            Assert.False(task.IsFaulted, "Expected task to be running but it was faulted.");
            Assert.False(task.IsCanceled, "Expected task to be running but it was canceled.");
        }

        public static void AssertCompletedSuccessfully(this Task task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));
            Assert.True(task.IsCompleted, "Expected task to be completed but it was not.");
            Assert.False(task.IsFaulted, "Expected task to be completed but it was faulted.");
            Assert.False(task.IsCanceled, "Expected task to be completed but it was canceled.");
        }

        public static void AssertCanceled(this Task task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));
            Assert.True(task.IsCompleted, "Expected task to be canceled but it was running.");
            Assert.False(task.IsFaulted, "Expected task to be canceled but it was faulted.");
            Assert.True(task.IsCanceled, "Expected task to be canceled but it was not.");
        }
    }
}
