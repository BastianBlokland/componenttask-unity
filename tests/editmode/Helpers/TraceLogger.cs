using System.Diagnostics;

namespace ComponentTask.Tests.EditMode.Helpers
{
    /// <summary>
    /// Logger to output messages of <see cref="System.Diagnostics.Debug"/> and
    /// <see cref="System.Diagnostics.Trace"/> to the unity error log.
    ///
    /// This is usefull as it will cause a failure in 'Debug.Assert' or 'Debug.Fail' to fail tests.
    /// </summary>
    public static class TraceLogger
    {
        private sealed class LogTraceListener : TraceListener
        {
            public override void Write(string message) =>
                UnityEngine.Debug.LogError($"TRACE: '{message}'");

            public override void WriteLine(string message) =>
                UnityEngine.Debug.LogError($"TRACE: '{message}'");
        }

        private static bool registered;

        public static void Register()
        {
            if (registered)
                return;

            Trace.Listeners.Add(new LogTraceListener());
            registered = true;
        }
    }
}
