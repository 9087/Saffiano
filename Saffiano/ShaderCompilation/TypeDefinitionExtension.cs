using System;
using System.Collections.Generic;
using Mono.Cecil;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    internal static class TypeDefinitionExtension
    {
        public static string GetUniformFullName(this TypeDefinition td)
        {
            string name = td.Name;
            while (td.DeclaringType != null)
            {
                name = td.DeclaringType.Name + "+" + name;
                td = td.DeclaringType;
            }
            return td.Namespace + "." + name;
        }

        public static Type GetRuntimeType(this TypeDefinition typeDefinition)
        {
            var assembly = AppDomain.CurrentDomain.Load(typeDefinition.Module.Assembly.FullName);
            return assembly.GetType(typeDefinition.GetUniformFullName());
        }

        public static MethodReference FindMethod(this TypeDefinition typeDefinition, string methodName)
        {
            var list = typeDefinition.Methods.Where((md) => md.Name == methodName);
            return list.Any() ? list.First() : null;
        }

        public static PropertyDefinition FindPropertyDefinitionIncludeAncestors(this TypeDefinition typeDefinition, string name)
        {
            while (typeDefinition != null)
            {
                foreach (var property in typeDefinition.Properties)
                {
                    if (property.Name == name)
                    {
                        return property;
                    }
                }
                typeDefinition = typeDefinition.BaseType.Resolve();
            }
            return null;
        }
    }
}
