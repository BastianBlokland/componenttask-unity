using System;
using System.Diagnostics;
using System.Threading;
using ComponentTask.Exceptions;

namespace ComponentTask.Internal
{
    internal static class UnityHelper
    {
        private static int unityManagedThreadId = -1;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            unityManagedThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static void ThrowForNonUnityThreadOrEditMode()
        {
            // 'unityManagedThreadId' of '-1' means we have not yet loaded playmode.
            if (unityManagedThreadId < 0)
                throw new NotPlayingException();

            if (Thread.CurrentThread.ManagedThreadId != unityManagedThreadId)
                throw new NonUnityThreadException();

#if UNITY_EDITOR
            // This catches the case where the application has been stopped in the editor.
            if (!UnityEngine.Application.isPlaying)
                throw new NotPlayingException();
#endif
        }

        public static void ThrowForInvalidObjectParam(UnityEngine.Object reference, string paramName)
        {
            /* This follows the unity standard where 'null' throws a null-ref and the unity-side
            being destroyed throws a 'UnityEngine.MissingReferenceException'.

            Unity has another behaviour when you define an inspector param but don't assign
            it and then access it: that throws a 'UnityEngine.UnassignedReferenceException', but
            i don't know how to implement that without access to the serialization internals. */

            if (reference is null)
                throw new ArgumentNullException(paramName);
            if (!reference)
                ThrowMissingReference(reference);
        }

        public static void ThrowMissingReference(UnityEngine.Object reference)
        {
            Debug.Assert(!(reference is null), "Reference is null");
            throw new UnityEngine.MissingReferenceException($"The object of type '{reference.GetType().Name}' has been destroyed but you are still trying to access it.");
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
    }
}
