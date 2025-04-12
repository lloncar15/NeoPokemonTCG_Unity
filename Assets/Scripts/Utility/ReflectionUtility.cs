using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GimGim.Utility
{
    /// <summary>
    /// A collection of utility methods that can help with using reflection for calling methods.
    /// Uses a method cache to try to mitigate the performance impact of using reflection.
    /// </summary>
    public static class ReflectionUtility {
        
        private static readonly Dictionary<string, MethodInfo> MethodCache = new();

        /// <summary>
        /// Returns a generic method of the specified name for the specified type and generic argument types.
        /// If the type doesn't have a method with the specified name and generic argument count, returns null.
        /// </summary>
        public static MethodInfo GetGenericMethod(Type type, string methodName, params Type[] argTypes) {
            string methodKey = MakeKey(type, methodName, argTypes);
            if (!MethodCache.TryGetValue(methodKey, out MethodInfo result)) {
                MethodInfo baseMethod = GetGenericBaseMethod(type, methodName, argTypes.Length);
                result = baseMethod.MakeGenericMethod(argTypes);
                MethodCache.Add(methodKey, result);
            }

            return result;
        }

        /// <summary>
        /// Returns a method of the specified name for the specified type.
        /// If the type doesn't have a method with the specified name, returns null.
        /// </summary>
        public static MethodInfo GetMethod(Type type, string methodName) {
            string baseMethodKey = MakeKey(type, methodName);
            if (!MethodCache.TryGetValue(baseMethodKey, out MethodInfo result)) {
                MethodInfo[] methodInfos = type.GetMethods();
                result = methodInfos.FirstOrDefault(method => method.Name == methodName);
                MethodCache.Add(baseMethodKey, result);
            }

            return result;
        }

        private static string MakeKey(Type type, string methodName, params Type[] argTypes) {
            StringBuilder keyBuilder = new($"{type.FullName}${methodName}");
            foreach (Type argType in argTypes) {
                keyBuilder.Append($"?{argType.FullName}");
            }

            return keyBuilder.ToString();
        }

        private static MethodInfo GetGenericBaseMethod(Type type, string methodName, int genericArgCount = 0) {
            string baseMethodKey = MakeKey(type, methodName) + $"<{genericArgCount}>";
            if (!MethodCache.TryGetValue(baseMethodKey, out MethodInfo result)) {
                MethodInfo[] methodInfos = type.GetMethods();
                result = methodInfos.FirstOrDefault(method =>
                    method.Name == methodName && method.GetGenericArguments().Length == genericArgCount);
                MethodCache.Add(baseMethodKey, result);
            }

            return result;
        }
    }
}