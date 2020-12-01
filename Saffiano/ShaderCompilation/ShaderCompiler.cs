using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Saffiano.ShaderCompilation
{
    public class ShaderCompiler
    {
        internal EvaluationStack evaluationStack { get; private set; } = new EvaluationStack();

        internal VariableAllocator internalVariableAllocator { get; private set; } = new VariableAllocator("internal");

        internal VariableAllocator localVariableAllocator { get; private set; } = new VariableAllocator("local");

        public ShaderCompiler()
        {
        }

        public string Compile(MethodReference methodReference, out HashSet<Uniform> uniforms)
        {
            if (methodReference == null)
            {
                uniforms = null;
                return null;
            }

            var methodDefinition = methodReference.Resolve();

            // Method body

            // Reference: ECMA-335
            // http://www.ecma-international.org/publications/standards/Ecma-335.htm

            CompileContext compileContext = new CompileContext(methodReference);

            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                instruction.Step(compileContext);
            }
            uniforms = compileContext.uniforms;

            return compileContext.shadercode;
        }
    }
}
