using System;

namespace ComponentTask.Tests.EditMode.Exceptions
{
    public sealed class TestException : Exception
    {
        internal TestException()
            : base($"Exception that is part of the test-suite")
        {
        }
    }
}
