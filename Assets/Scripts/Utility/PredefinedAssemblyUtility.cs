using System;
using System.Collections.Generic;
using System.Linq;

namespace GimGim.Utility {
    /// <summary>
    /// A utility class for working with predefined assemblies in the current application domain.
    /// Provides methods to retrieve types from specific assemblies based on their relationship to a given interface or base type.
    /// </summary>
    public static class PredefinedAssemblyUtility {
        private static readonly HashSet<string> TrustedAssemblies = new() {
            "Assembly-CSharp",
            "Assembly-CSharp-firstpass",
            "Assembly-CSharp-Editor",
            "Assembly-CSharp-Editor-firstpass"
        };

        /// <summary>
        /// Adds types from a given assembly to the result collection if they implement or inherit from the specified interface or base type.
        /// </summary>
        /// <param name="assemblyTypes">The array of types in the assembly.</param>
        /// <param name="interfaceType">The interface or base type to match.</param>
        /// <param name="result">The collection to which matching types will be added.</param>
        private static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> result) {
            if (assemblyTypes is null) return;
            foreach (var type in assemblyTypes) {
                if (type != interfaceType && interfaceType.IsAssignableFrom(type)) {
                    result.Add(type);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of types from predefined assemblies that implement or inherit from the specified interface or base type.
        /// </summary>
        /// <param name="interfaceType">The interface or base type to match.</param>
        /// <returns>A list of types that match the specified interface or base type.</returns>
        public static List<Type> GetTypes(Type interfaceType) {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => TrustedAssemblies.Contains(assembly.GetName().Name))
                .ToArray();

            var types = new List<Type>();
            foreach (var assembly in assemblies) {
                AddTypesFromAssembly(assembly.GetTypes(), interfaceType, types);
            }

            return types;
        }
    }
}