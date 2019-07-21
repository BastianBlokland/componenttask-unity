using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ComponentTask.Tests.EditMode.Mocks
{
    public sealed class MockExceptionHandler : IExceptionHandler
    {
        private readonly Queue<Exception> exceptionQueue = new Queue<Exception>();

        public void Handle(Exception exception)
        {
            if (exception == null)
                Assert.Fail("Null exception was raised");
            this.exceptionQueue.Enqueue(exception);
        }

        public void AssertPop<TException>() where TException : Exception
        {
            Assert.NotZero(exceptionQueue.Count, "No exception left to pop");
            var ex = exceptionQueue.Dequeue();
            Assert.True(typeof(TException).IsAssignableFrom(ex.GetType()));
        }

        public void AssertNoExceptions()
        {
            Assert.Zero(exceptionQueue.Count, "Unexpected exception occurred");
        }
    }
}
