using System;

namespace ComponentTask.Internal
{
    internal static class DelegateExtensions
    {
        /// <summary>
        /// Get a diagnostic identifier that best describes this delegate.
        /// </summary>
        /// <remarks>
        /// Only meant for diagnostic purposes, do NOT depend on output being in a specific format.
        /// </remarks>
        /// <param name="@delegate">Delegate to get an identifier for.</param>
        /// <returns>Identifier best describing the given delegate.</returns>
        public static string GetDiagIdentifier(this Delegate @delegate)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            try
            {
                var method = @delegate.Method;
                var owner = method.DeclaringType;
                return $"{owner.Name}.{method.Name}";
            }
            catch (MemberAccessException)
            {
                if (@delegate.Target != null)
                    return $"{@delegate.Target.GetType().Name}.<private>";
                return "<private>";
            }
        }
    }
}
