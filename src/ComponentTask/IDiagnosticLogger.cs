namespace ComponentTask
{
    /// <summary>
    /// Interface for logging diagnostic output.
    /// </summary>
    /// <remarks>
    /// Can be used to get diagnostic output when using a custom <see cref="ComponentTask.LocalTaskRunner"/>.
    /// </remarks>
    public interface IDiagnosticLogger
    {
        /// <summary>
        /// Log a diagnostic message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Log(string message);
    }
}
