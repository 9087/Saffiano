using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal static class TypeReferenceExtension
    {
        public static TypeDefinition ResolveWithGenericInstanceType(this TypeReference tr, GenericInstanceType git)
        {
            TypeDefinition td = tr.Resolve();
            if (td != null)
            {
                return td;
            }

            if (tr is GenericParameter && git != null && tr.DeclaringType == git.ElementType)
            {
                return (git.GenericArguments[(tr as GenericParameter).Position]).Resolve();
            }
            if (tr is ArrayType && git != null)
            {
                var elementTypeReference = tr.GetElementType();
            }
            return null;
        }
    }
}
