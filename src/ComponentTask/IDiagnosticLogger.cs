namespace ComponentTask
{
    /// <summary>
    /// Interface for logging diagnostic output.
    /// </summary>
    public interface IDiagnosticLogger
    {
        /// <summary>
        /// Log a diagnostic message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Log(string message);
    }
}
