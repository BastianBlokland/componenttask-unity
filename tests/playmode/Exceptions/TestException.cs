using System;

namespace ComponentTask.Tests.PlayMode.Exceptions
{
    public sealed class TestException : Exception
    {
        private const string message = "Exception that is part of the test-suite. Don't worry it's expected that this is thrown.";

        internal TestException()
            : base(message)
        {
        }

        public static void ExpectLog() =>
            UnityEngine.TestTools.LogAssert.Expect(
                type: UnityEngine.LogType.Exception,
                message: $"{nameof(TestException)}: {message}");
    }
}
