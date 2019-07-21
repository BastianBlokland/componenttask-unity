using System.Diagnostics;

namespace ComponentTask.Internal
{
    /// <summary>
    /// Logger to output messages of <see cref="System.Diagnostics.Debug"/> and
    /// <see cref="System.Diagnostics.Trace"/> to the unity error log.
    ///
    /// This is usefull as it will cause a failure in 'Debug.Assert' or 'Debug.Fail' to be visible
    /// in the editor.
    /// </summary>
    /// <remarks>
    /// Is automatically registered at 'RuntimeInitializeLoadType.AfterSceneLoad' if none is registered,
    /// if you want to use your own then register it before then.
    /// </remarks>
    internal static class TraceLogger
    {
        private sealed class UnityLogTraceListener : System.Diagnostics.TraceListener
        {
            public override void Write(string message) =>
                UnityEngine.Debug.LogError($"TRACE: '{message}'");

            public override void WriteLine(string message) =>
                UnityEngine.Debug.LogError($"TRACE: '{message}'");
        }

        private static bool registered;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (registered)
                return;

            /* Only add a new listener if none is setup yet. This combined with the fact that this
            runs AFTER scene load and not before should give projects the chance to use their own
            trace-listener instead of this one. */

            if (Trace.Listeners.Count == 0)
                Trace.Listeners.Add(new UnityLogTraceListener());

            registered = true;
        }
    }
}
