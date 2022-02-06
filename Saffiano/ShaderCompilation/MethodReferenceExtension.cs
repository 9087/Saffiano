using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    public static class MethodReferenceExtension
    {
        public static MethodBase GetMethodInfoWithGenericInstanceType(this MethodReference methodReference, GenericInstanceType git)
        {
            if (git == null)
            {
                return methodReference.GetMethodInfo();
            }
            var typeDefinition = git.Resolve();
            return methodReference.GetMethodInfo(typeDefinition, git);
        }

        public static MethodBase GetMethodInfo(this MethodReference methodReference)
        {
            return methodReference.GetMethodInfo(methodReference.DeclaringType.Resolve(), null);
        }

        private static MethodBase GetMethodInfo(this MethodReference methodReference, TypeDefinition typeDefinition, GenericInstanceType git)
        {
            var methodDefinition = methodReference.Resolve();
            var declaringType = typeDefinition.GetRuntimeType();
            if (git != null && !methodDefinition.IsConstructor)
            {
                return declaringType.GetMethod(methodReference.Name);
            }
            var parameterTypes = methodReference.Parameters.Select((pd) => {
                var td = pd.ParameterType.ResolveWithGenericInstanceType(git);
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
