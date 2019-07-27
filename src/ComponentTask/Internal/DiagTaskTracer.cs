using System;
using System.Diagnostics;
using System.Text;

namespace ComponentTask.Internal
{
    internal sealed class DiagTaskTracer
    {
        private readonly string identifier;
        private readonly IDiagnosticLogger logger;

        private DiagTaskTracer(IDiagnosticLogger logger, string identifier)
        {
            Debug.Assert(!string.IsNullOrEmpty(identifier), "Invalid identifier");
            Debug.Assert(!(logger is null), "Logger is null");

            this.identifier = identifier;
            this.logger = logger;
        }

        public void LogInvoked(object argument = null) =>
            this.logger.Log($"{identifier} -> Invoking (argument: '{argument?.ToString() ?? "none"}').");

        public void LogPaused() =>
            this.logger.Log($"{identifier} -> Paused.");

        public void LogResumed() =>
            this.logger.Log($"{identifier} -> Resumed.");

        public void LogCompletedSynchronouslyAsSuccess(object result) =>
            this.logger.Log($"{identifier} -> Completed synchronously: Success (result: '{result?.ToString() ?? "none"}').");

        public void LogCompletedSynchronouslyAsFaulted(Exception exception) =>
            this.logger.Log($"{identifier} -> Completed synchronously: Faulted (exception: '{GetExceptionMessage(exception) ?? "none"}').");

        public void LogCompletedSynchronouslyAsCanceled() =>
            this.logger.Log($"{identifier} -> Completed synchronously: Canceled.");

        public void LogStartRunning() =>
            this.logger.Log($"{identifier} -> Started running asynchronously.");

        public void LogCompletedAsSuccess(object result) =>
            this.logger.Log($"{identifier} -> Completed: Success (result: '{result?.ToString() ?? "none"}').");

        public void LogCompletedAsFaulted(Exception exception) =>
            this.logger.Log($"{identifier} -> Completed: Faulted (exception: '{GetExceptionMessage(exception) ?? "none"}').");

        public void LogCompletedAsCanceled() =>
            this.logger.Log($"{identifier} -> Completed: Canceled.");

        public void LogCanceled() =>
            this.logger.Log($"{identifier} -> Canceled.");

        public static DiagTaskTracer Create(IDiagnosticLogger logger, Delegate taskCreator)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));
            if (taskCreator is null)
                throw new ArgumentNullException(nameof(taskCreator));

            return new DiagTaskTracer(logger, taskCreator.GetDiagIdentifier());
        }

        private static string GetExceptionMessage(Exception exception)
        {
            if (exception is null)
                return null;

            if (exception is AggregateException aggregateException)
            {
                var exceptions = aggregateException.Flatten().InnerExceptions;
                if (exceptions.Count == 1)
                    return exceptions[0].Message;

                var msgBuilder = new StringBuilder();
                for (int i = 0; i < exceptions.Count; i++)
                {
                    if (i != 0)
                        msgBuilder.Append("\n");
                    msgBuilder.Append(exceptions[i].Message);
                }

                return msgBuilder.ToString();
            }
            else
                return exception.Message;
        }
    }
}
