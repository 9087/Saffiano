using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    internal class CompileContext
    {
        private EvaluationStack evaluationStack = new EvaluationStack();
        private TextWriter writer = new StringWriter();
        private VariableAllocator internalAllocator = new VariableAllocator("internal");
        private VariableAllocator localAllocator = new VariableAllocator("local");

        public HashSet<Uniform> uniforms { get; private set; } = new HashSet<Uniform>();

        private string GetUniformDefinitions()
        {
            var uniformDefinitions = new StringWriter();
            foreach (var uniform in this.uniforms)
            {
                uniformDefinitions.WriteLine(Format("uniform {0} {1};", uniform.propertyInfo.PropertyType, uniform.propertyInfo.Name));
            }
            return uniformDefinitions.ToString();
        }

        private string GetStatement()
        {
            return writer.ToString();
        }

        private string GetAttributeDefinitions()
        {
            var attributeDefinitions = new StringWriter();
            var methodInfo = this.methodDefinition.GetMethodInfo();
            var parameterInfos = methodInfo.GetParameters();
            foreach (var parameterInfo in parameterInfos)
            {
                Type elementType = null;
                if (parameterInfo.ParameterType.IsByRef)
                {
                    elementType = parameterInfo.ParameterType.GetElementType();
                }
                else
                {
                    elementType = parameterInfo.ParameterType;
                }
                if (!parameterInfo.IsOut)
                {
                    var attributes = parameterInfo.GetCustomAttributes(typeof(AttributeAttribute), true);
                    if (attributes.Length == 1)
                    {
                        var attribute = attributes[0] as AttributeAttribute;
                        attributeDefinitions.WriteLine(Format("layout (location = {0}) in {1} {2};", (uint)attribute.type, elementType, parameterInfo.Name));
                    }
                    else
                    {
                        attributeDefinitions.WriteLine(Format("in {0} {1};", elementType, parameterInfo.Name));
                    }
                }
                if (parameterInfo.IsOut)
                {
                    attributeDefinitions.WriteLine(Format("out {0} {1};", elementType, parameterInfo.Name));
                }
            }
            return attributeDefinitions.ToString();
        }

        public string shadercode
        {
            get
            {
                var code = new StringWriter();
                code.WriteLine("#version 330 core");
                code.WriteLine(GetUniformDefinitions());
                code.WriteLine(GetAttributeDefinitions());
                code.WriteLine("void main() {");
                code.WriteLine(GetStatement());
                code.WriteLine("}");
                return code.ToString();
            }
        }

        public TypeDefinition declaringType
        {
            get;
            private set;
        }

        public Collection<ParameterDefinition> parameters => methodDefinition.Parameters;

        private MethodDefinition methodDefinition;

        public CompileContext(MethodReference methodReference)
        {
            methodDefinition = methodReference.Resolve();
            declaringType = methodDefinition.DeclaringType;
        }

        public Value AllocateInternal(TypeReference type)
        {
            return internalAllocator.Allocate(type);
        }

        public Value AllocateLocal(TypeReference type, uint index)
        {
            return localAllocator.Allocate(type, index);
        }

        public Value GetLocal(uint index)
        {
            return localAllocator.Get(index);
        }

        public Value Push(TypeReference type, object name)
        {
            return evaluationStack.Push(type, name);
        }

        public Value Push(Value value)
        {
            evaluationStack.Push(value);
            return value;
        }

        public Value Push(ParameterReference parameterReference)
        {
            return evaluationStack.Push(parameterReference);
        }

        public Value Push(PropertyReference propertyReference)
        {
            return evaluationStack.Push(propertyReference);
        }

        public List<Value> Pop(int count)
        {
            return evaluationStack.Pop(count);
        }

        public Value Pop()
        {
            return evaluationStack.Pop();
        }

        public Value Peek()
        {
            return evaluationStack.Peek();
        }

        public bool AddUniform(Uniform uniform)
        {
            return uniforms.Add(uniform);
        }

        public string Format(string format, params object[] args)
        {
            List<object> parameters = new List<object>();
            foreach (var arg in args)
            {
                if (arg is TypeReference)
                {
                    parameters.Add(Type(arg as TypeReference));
                }
                else if (arg is PropertyReference)
                {
                    throw new Exception();
                }
                else if (arg is string)
                {
                    parameters.Add(arg);
                }
                else if (arg is Value)
                {
                    parameters.Add((arg as Value).ToString());
                }
                else if (arg is Type)
                {
                    parameters.Add(Type((Type)arg));
                }
                else if (arg.GetType().IsValueType)
                {
                    parameters.Add(arg.ToString());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return string.Format(format, parameters.ToArray());
        }

        public string Property(Value @this, PropertyReference propertyReference)
        {
            var methodDefinition = propertyReference.Resolve().GetMethod;
            var methodInfo = methodDefinition.GetMethodInfo();
            var shaderAttributes = methodInfo.GetCustomAttributes<ShaderAttribute>();
            string pattern;
            if (shaderAttributes.Any())
            {
                // convert to method call
                pattern = shaderAttributes.First().pattern;
            }
            else
            {
                pattern = "{0}.{1}";
            }
            return string.Format(pattern, @this.name, propertyReference.Name);
        }

        public string Type(Type type)
        {
            return Type(type.GetTypeDefinition());
        }

        public string Type(TypeReference typeReference)
        {
            // built-in shader type
            var type = typeReference.Resolve().GetRuntimeType();
            if (type == typeof(float))
            {
                return "float";
            }
            var shaderAttributes = type.GetCustomAttributes<ShaderAttribute>();
            if (!shaderAttributes.Any())
            {
                Debug.LogErrorFormat("Type \"{0}\" is not a built-in shader type.", type.FullName);
                return type.FullName;
            }
            if (shaderAttributes.Count() > 1)
            {
                throw new Exception();
            }
            return shaderAttributes.First().pattern;
        }

        public string Join<T>(String separator, IEnumerable<T> values)
        {
            return string.Join(separator, values);
        }

        public string Method(MethodReference methodReference, Value[] parameters)
        {
            // built-in shader method
            var methodInfo = methodReference.GetMethodInfo();
            var shaderAttributes = methodInfo.GetCustomAttributes<ShaderAttribute>();
            string pattern = null;
            if (shaderAttributes.Any())
            {
                pattern = shaderAttributes.First().pattern;
            }
            var md = methodReference.Resolve();
            if (md.IsConstructor)
            {
                var parameterPatterns = new List<string>();
                for (int i = 0; i < md.Parameters.Count; i++)
                {
                    parameterPatterns.Add(string.Format("{{{0}}}", i));
                }
                pattern = Format("{0}({1})", md.DeclaringType, Join(", ", parameterPatterns));
            }
            if (pattern != null)
            {
                return Format(pattern, parameters);
            }
            Debug.LogErrorFormat("Method \"{0}.{1}\" is not a built-in shader method.", methodInfo.DeclaringType.FullName, methodInfo.Name);
            return null;
        }

        public string Method(MethodReference methodReference, List<Value> parameters)
        {
            return Method(methodReference, parameters.ToArray());
        }

        private string Assign(Value target, object value)
        {
            string format;
            if (target.initialized)
            {
                format = "{1} = {2};";
            }
            else
            {
                format = "{0} {1} = {2};";
                target.initialized = true;
            }
            return Format(format, target.type, target.name, value);
        }

        public void Assign(Value target, object value, CompileContext compileContext)
        {
            writer.WriteLine(Assign(target, value));
        }
    }
}
