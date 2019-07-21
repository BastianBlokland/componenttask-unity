using System.Diagnostics;

namespace ComponentTask.Tests.EditMode.Helpers
{
    public static class TraceAsserter
    {
        private sealed class FailTraceListener : TraceListener
        {
            public override void Write(string message) =>
                NUnit.Framework.Assert.Fail($"TRACE: '{message}'");

            public override void WriteLine(string message) =>
                NUnit.Framework.Assert.Fail($"TRACE: '{message}'");
        }

        private static bool registered;

        public static void Register()
        {
            if (registered)
                return;

            Trace.Listeners.Add(new FailTraceListener());
            registered = true;
        }
    }
}
