using ComponentTask;
using ComponentTask.Internal;

namespace UnityEngine
{
    /// <summary>
    /// Extensions for <see cref="UnityEngine.GameObject"/>.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Create a <see cref="ITaskRunner"/> on the given gameobject.
        /// </summary>
        /// <remarks>
        /// If you run tasks on the returned runner then they are cancelled automatically when the
        /// gameobject is destroyed.
        ///
        /// Can only be called from the unity-thread in play-mode.
        /// </remarks>
        /// <exception cref="ComponentTask.Exceptions.NotPlayingException">
        /// Thrown when called in edit-mode.
        /// </exception>
        /// <exception cref="ComponentTask.Exceptions.NonUnityThreadException">
        /// Thrown when called from a non-unity thread.
        /// </exception>
        /// <param name="gameObject">GameObject to create the task-runner on.</param>
        /// <returns>Newly created <see cref="ITaskRunner"/>.</returns>
        public static ITaskRunner CreateTaskRunner(this GameObject gameObject)
        {
            // Validate params.
            UnityHelper.ThrowForInvalidObjectParam(gameObject, nameof(gameObject));

            // Validate state.
            UnityHelper.ThrowForNonUnityThreadOrEditMode();

            // Create runner.
            var runner = gameObject.AddComponent<MonoBehaviourTaskRunner>();
            runner.hideFlags = HideFlags.HideInInspector;

            return runner;
        }
    }
}
