using Mono.Cecil;
using Mono.Cecil.Cil;
using Saffiano.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    public class ShaderCompiler
    {
        public ShaderCompiler()
        {
        }

        public string Compile(MethodReference methodReference, out HashSet<Uniform> uniforms, Func<ParameterInfo, bool> filter = null)
        {
            if (methodReference == null)
            {
                uniforms = null;
                return null;
            }

            // Method body

            // Reference: ECMA-335
            // http://www.ecma-international.org/publications/standards/Ecma-335.htm
            
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, MethodDefinition> methods = new Dictionary<string, MethodDefinition>();
            methods.Add("main", methodReference.Resolve());
            Dictionary<string, CompileContext> contexts = new Dictionary<string, CompileContext>();
            List<string> sources = new List<string>();

            uniforms = new HashSet<Uniform>();
            var shaderExtensions = new HashSet<ShaderExtension>();

            while (methods.Count != 0)
            {
                Dictionary<string, MethodDefinition> tmp = new Dictionary<string, MethodDefinition>();
                foreach (var name in methods.Keys)
                {
                    if (visited.Contains(name))
                    {
                        continue;
                    }
                    var md = methods[name];
                    CompileContext cc = new CompileContext(md);
                    contexts[name] = cc;
                    cc.Generate();
                    uniforms.UnionWith(cc.uniforms);
                    shaderExtensions.UnionWith(cc.GetShaderExtensions());
                    sources.Insert(0, cc.GetMethodSourceCode(name));
                    visited.Add(name);
                    foreach (var n in cc.methods.Keys)
                    {
                        if (visited.Contains(n))
                        {
                            continue;
                        }
                        tmp[n] = cc.methods[n];
                    }
                }
                methods.Clear();
                foreach (var name in tmp.Keys)
                {
                    methods[name] = tmp[name];
                }
            }

            var code = new StringWriter();
            code.WriteLine(CompileContext.GetUniformSourceCode(uniforms));
            code.WriteLine(contexts["main"].GetAttributeSourceCode(filter: filter));
            code.WriteLine(CompileContext.GetShaderExtensionSourceCode(shaderExtensions));
            foreach (var source in sources)
            {
                code.WriteLine(source);
            }
            
            return code.ToString();
        }
    }
}
