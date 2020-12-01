using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    public static class MethodReferenceExtension
    {
        public static MethodBase GetMethodInfo(this MethodReference methodReference)
        {
            var methodDefinition = methodReference.Resolve();
            var typeDefinition = methodDefinition.DeclaringType;
            var declaringType = typeDefinition.GetRuntimeType();
            var parameterTypes = methodReference.Parameters.Select((pd) => {
                var td = pd.ParameterType.Resolve();
                var pt = td.GetRuntimeType();
                if (pd.ParameterType.IsPointer)
                {
                    return pt.MakePointerType();
                }
                if (pd.ParameterType.IsByReference)
                {
                    return pt.MakeByRefType();
                }
                return pt;
            }).ToArray();
            BindingFlags all = Enum.GetValues(typeof(BindingFlags)).Cast<BindingFlags>().Aggregate((a, b) => a | b);
            if (methodDefinition.IsConstructor)
            {
                return declaringType.GetConstructor(all, null, parameterTypes, null);
            }
            return declaringType.GetMethod(methodReference.Name, all, null, parameterTypes, null);
        }
    }
}
