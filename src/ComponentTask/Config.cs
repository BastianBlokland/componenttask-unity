using UnityEngine;

namespace ComponentTask
{
    /// <summary>
    /// Global configuration.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Should diagnostics be enabled.
        /// </summary>
        /// <remarks>
        /// Runs all tasks with the '<see cref="TaskRunOptions.DiagnosticLogging"/>' flag.
        /// </remarks>
        /// <value>True if diagnostics are active, otherwise false.</value>
        public static bool GlobalDiagnosticsActive { get; set; }

        /// <summary>
        /// Modify run-options based on the global config.
        /// </summary>
        /// <param name="options">Options to modify.</param>
        /// <returns>Modified options.</returns>
        internal static TaskRunOptions Apply(TaskRunOptions options)
        {
            if (GlobalDiagnosticsActive)
                options |= TaskRunOptions.DiagnosticLogging;

            return options;
        }
    }
}
