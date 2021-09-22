using Mono.Cecil;
using Mono.Cecil.Cil;
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
        private TextWriter writer = new StringWriter();

        private EvaluationStack evaluationStack = new EvaluationStack();
        private CodeBlockStack codeBlockStack = new CodeBlockStack();
        private VariableAllocator internalAllocator = new VariableAllocator("internal");
        private VariableAllocator localAllocator = new VariableAllocator("local");

        public HashSet<Uniform> uniforms { get; private set; } = new HashSet<Uniform>();

        public Dictionary<string, MethodDefinition> methods { get; private set; } = new Dictionary<string, MethodDefinition>();

        public string GetUniformSourceCode()
        {
            return GetUniformSourceCode(this.uniforms);
        }

        public static string GetUniformSourceCode(HashSet<Uniform> uniforms)
        {
            var uniformDefinitions = new StringWriter();
            foreach (var uniform in uniforms)
            {
                uniformDefinitions.WriteLine(Format("uniform {0} {1};", uniform.propertyInfo.PropertyType, uniform.propertyInfo.Name));
            }
            return uniformDefinitions.ToString();
        }

        public string GetAttributeSourceCode()
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

        public string GetMethodSourceCode(string methodName)
        {
            var code = new StringWriter();
            code.WriteLine(string.Format("void {0}() {{", methodName));
            code.WriteLine(writer.ToString());
            code.WriteLine("}");
            return code.ToString();
        }

        public TypeDefinition declaringType
        {
            get;
            private set;
        }

        public Collection<ParameterDefinition> parameters => methodDefinition.Parameters;

        private MethodDefinition methodDefinition { get; set; }

        private bool processed = false;

        public CompileContext(MethodReference methodReference)
        {
            methodDefinition = methodReference.Resolve();
            declaringType = methodDefinition.DeclaringType;
        }

        public void Generate()
        {
            Debug.Assert(processed == false);
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                instruction.Step(this);
            }
            processed = true;
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

        public void Begin(BlockType blockType, Instruction start, Instruction end)
        {
            codeBlockStack.Push(new Block(blockType, start, end));
            writer.WriteLine("{");
        }

        public bool TryEnd(Instruction current)
        {
            if (codeBlockStack.Count == 0)
            {
                return false;
            }
            if (GetPeekCodeBlock().end == current)
            {
                End();
            }
            return true;
        }

        public bool End()
        {
            writer.WriteLine("}");
            codeBlockStack.Pop();
            return true;
        }

        public Block GetPeekCodeBlock()
        {
            return codeBlockStack.Peek();
        }

        public BlockType GetPeekCodeBlockType()
        {
            return GetPeekCodeBlock().blockType;
        }

        public bool AddUniform(Uniform uniform)
        {
            return uniforms.Add(uniform);
        }

        public static string Format(string format, params object[] args)
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

        public Value Field(Value @this, FieldDefinition fieldDefinition)
        {
            return new Value(fieldDefinition.FieldType, string.Format("{0}.{1}", @this.name, fieldDefinition.Name));
        }

        public static string Type(Type type)
        {
            return Type(type.GetTypeDefinition());
        }

        public static string Type(TypeReference typeReference)
        {
            // built-in shader type
            var type = typeReference.Resolve().GetRuntimeType();
            if (type.IsValueType)
            {
                if (type == typeof(bool))  return "bool" ;
                if (type == typeof(int))   return "int"  ;
                if (type == typeof(float)) return "float";
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
            var callerDeclaringType = methodDefinition.DeclaringType;
            var calleeDeclaringType = methodReference.DeclaringType.Resolve();
            if (calleeDeclaringType.IsBaseOf(callerDeclaringType) || calleeDeclaringType == callerDeclaringType)
            {
                string methodName = Format("{0}_{1}", callerDeclaringType.Name, methodReference.Resolve().Name);
                methods[methodName] = methodReference.Resolve();
                return Format("{0}();", methodName);
            }
            Debug.LogErrorFormat("Method \"{0}.{1}\" is not a built-in shader method.", methodInfo.DeclaringType.FullName, methodInfo.Name);
            return null;
        }

        public string Method(MethodReference methodReference, List<Value> parameters)
        {
            return Method(methodReference, parameters.ToArray());
        }

        public void Assign(Value target, object value)
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
            var s = Format(format, target.type, target.name, value);
            writer.WriteLine(s);
        }

        public void If(Value value)
        {
            writer.WriteLine("if({0})", value);
        }

        public void Else()
        {
            writer.WriteLine("else");
        }

        public void WriteLine(string content)
        {
            writer.WriteLine(content);
        }
    }
}
