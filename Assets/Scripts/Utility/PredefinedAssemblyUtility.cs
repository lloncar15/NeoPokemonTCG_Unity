using System;
using System.Collections.Generic;
using System.Reflection;

namespace GimGim.Utility {
    public static class PredefinedAssemblyUtility {
        enum AssemblyType {
            AssemblyCSharp,
            AssemblyCSharpEditor,
            AssemblyCSharpEditorFirstPass,
            AssemblyCSharpFirstPass
        }

        static AssemblyType? GetAssemblyType(string assemblyName) {
            return assemblyName switch {
                "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
                "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
                _ => null
            };
        }

        static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> result) {
            if (assemblyTypes is null) return;
            for (int i = 0; i < assemblyTypes.Length; ++i) {
                Type type = assemblyTypes[i];
                if (type != interfaceType && interfaceType.IsAssignableFrom(type)) {
                    result.Add(type);
                }
            }
        }

        public static List<Type> GetTypes(Type interfaceType) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
            
            List<Type> types = new List<Type>();
            for (int i = 0; i < assemblies.Length; i++) {
                AssemblyType? assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
                if (assemblyType is not null) {
                    assemblyTypes.Add((AssemblyType) assemblyType, assemblies[i].GetTypes());
                }
            }
        
            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
            AddTypesFromAssembly(assemblyCSharpTypes, interfaceType, types);

            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharpFirstPass, out var assemblyCSharpFirstPassTypes);
            AddTypesFromAssembly(assemblyCSharpFirstPassTypes, interfaceType, types);
        
            return types;
        }
    }
}