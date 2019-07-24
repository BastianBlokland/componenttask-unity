using UnityEngine;
using System.Threading;
using NUnit.Framework;
using System.Reflection;

namespace ComponentTask.Tests.PlayMode.Helpers
{
    public static class ComponentContextAsserter
    {
        /// <summary>
        /// Assert that current execution is running in the context of the given component.
        /// </summary>
        /// <remarks>
        /// Uses implementation details to assert this behaviour but can be usefull to assert that
        /// sync-context is flowed correctly.
        /// </remarks>
        public static void AssertRunningInComponentContext(
            Component component,
            TaskRunOptions options = TaskRunOptions.Default)
        {
            var currentSyncContext = SynchronizationContext.Current;
            var compSyncContext = GetComponentSyncContext(component, options);

            Assert.True(
                condition: object.ReferenceEquals(compSyncContext, currentSyncContext),
                message: $"Not running in the context of '{component}'");
        }

        private static SynchronizationContext GetComponentSyncContext(
            Component component,
            TaskRunOptions options)
        {
            /* These are internal implementation details but is usefull to be able to assert
            that sync-context is flowed correctly. */

            var monoBehaviourRunner = component.GetTaskRunner(options);
            var taskRunnerImpl = monoBehaviourRunner.
                GetType().
                GetField("taskRunner", BindingFlags.Instance | BindingFlags.NonPublic).
                GetValue(monoBehaviourRunner);
            var context = taskRunnerImpl.
                GetType().
                GetField("context", BindingFlags.Instance | BindingFlags.NonPublic).
                GetValue(taskRunnerImpl);
            return context as SynchronizationContext;
        }
    }
}
