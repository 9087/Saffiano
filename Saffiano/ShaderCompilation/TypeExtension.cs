using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    public static class TypeExtension
    {
        public static TypeDefinition GetTypeDefinition(this Type type)
        {
            var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(type.Assembly.Location);
            Queue<TypeDefinition> queue = new Queue<TypeDefinition>();
            foreach (var t in assemblyDefinition.MainModule.Types)
            {
                queue.Enqueue(t);
            }
            while (queue.Count != 0)
            {
                var t = queue.Dequeue();
                if (t.Name == type.Name && type.FullName == t.GetUniformFullName())
                {
                    return t;
                }
                foreach (var nestedType in t.NestedTypes)
                {
                    queue.Enqueue(nestedType);
                }
            }
            return null;
        }
    }
}
