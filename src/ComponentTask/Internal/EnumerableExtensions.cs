using System;
using System.Collections.Generic;

namespace ComponentTask.Internal
{
    internal static class EnumerableExtensions
    {
        private static class ThreadStatic<T>
        {
            [ThreadStatic] public static List<T> invokeList;
        }

        /// <summary>
        /// Invoke the given action on all the items in the enumerable while holding the lock.
        /// </summary>
        /// <param name="enumerable">Enumerable with entries to invoke the action on.</param>
        /// <param name="lockObject">Object to lock on while iterating the enumerable.</param>
        /// <param name="action">Action to invoke on the entries.</param>
        public static void LockedInvoke<T>(this IEnumerable<T> enumerable, object lockObject, Action<T> action)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));
            if (lockObject is null)
                throw new ArgumentNullException(nameof(lockObject));
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            /*
            This first gathers all items in a separate collection before invoking, two reasons for this:
            1: We hold the lock for as short a time as possible.
            2: We avoid calling 'external' code while holding a lock (which could potentially deadlock.
            */

            // Get a thread-static list to use.
            if (ThreadStatic<T>.invokeList is null)
                ThreadStatic<T>.invokeList = new List<T>();
            else
                ThreadStatic<T>.invokeList.Clear();

            // Gather all items to invoke while holding the lock.
            lock (lockObject)
            {
                foreach (var val in enumerable)
                    ThreadStatic<T>.invokeList.Add(val);
            }

            foreach (var invokeVal in ThreadStatic<T>.invokeList)
                action.Invoke(invokeVal);
        }
    }
}
