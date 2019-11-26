/*  Workaround for: https://github.com/BastianBlokland/componenttask-unity/issues/20
Disable the trace-logger on the '.Net 4.x' api in combination with the 'mono' scripting backend.
Reason is that the code-stripper (on all settings except 'Disabled') removes part of
'System.Configuration' on which 'System.Diagnostics.TraceListener' depends.
Because upm packages do not support 'link.xml' files at this time there is no easy way to instruct
the stripper not to remove those. Disabling is acceptable in this case as its only used as a log-sink
for 'Debug.Assert' and 'Debug.Fail' and either of those triggering would mean there is a serious bug
in this repository. */
#if !(ENABLE_MONO && NET_4_6)

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

#endif // !(ENABLE_MONO && NET_4_6)
