using System;

namespace UnityEngine
{
    /// <summary>
    /// Flags for configuring how tasks are being run.
    /// </summary>
    [Flags]
    public enum TaskRunOptions : int
    {
        /// <summary>
        /// Default run options.
        /// </summary>
        /// <remarks>
        /// Tasks are updated in 'LateUpdate' when the component is enabled.
        /// </remarks>
        Default = 0,

        /// <summary>
        /// Tasks are updated even if the component is disabled.
        /// </summary>
        /// <remarks>
        /// Tasks are still NOT updated when the gameobject is disabled.
        /// </remarks>
        UpdateWhileComponentDisabled = 1 << 0,

        /// <summary>
        /// Enable diagnostic logging.
        /// </summary>
        DiagnosticLogging = 1 << 1,
    }
}
