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
            var td = typeDefinition;
            while (td != null)
            {
                var list = td.Methods.Where((md) => md.Name == methodName);
                if (list.Any())
                {
                    return list.First();
                }
                if (td.BaseType == null)
                {
                    break;
                }
                td = td.BaseType.Resolve();
            }
            return null;
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
