using System;

namespace GimGim.UnityExtensions {
    public static class StringExtensions {
        
        /// <summary>
        /// Determines whether the source string contains the target string.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <param name="target">The target string to find.</param>
        /// <param name="comparison">The string comparison function.</param>
        /// <returns>True if the target string is in the source string.</returns>
        public static bool Contains (this string source, string target, StringComparison comparison) {
            return source.IndexOf(target, comparison) >= 0;
        }
    }
}