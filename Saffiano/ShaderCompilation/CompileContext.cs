using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Saffiano.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    class LocalVariableOpCodeData
    {
        public OpCode setOpCode { get; private set; }

        public OpCode[] getOpCodes { get; private set; }

        public uint? index { get; private set; }

        public LocalVariableOpCodeData(OpCode setOpCode, OpCode[] getOpCodes, uint? index)
        {
            this.setOpCode = setOpCode;
            this.getOpCodes = getOpCodes;
            this.index = index;
        }
    }

    internal class CompileContext
    {
        private TextWriter writer = new StringWriter();

        private EvaluationStack evaluationStack = new EvaluationStack();

        private CodeBlockStack codeBlockStack = new CodeBlockStack();
        private VariableAllocator allocator = new VariableAllocator("local");

        public HashSet<Uniform> uniforms { get; private set; } = new HashSet<Uniform>();

        public Dictionary<string, MethodDefinition> methods { get; private set; } = new Dictionary<string, MethodDefinition>();

        public EvaluationStack GetEvaluationStack()
        {
            return evaluationStack;
        }

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
            MethodBase methodInfo;

            // Geometry shader
            if (this.methodDefinition.Name == ShaderType.GeometryShader.ToString())
            {
                methodInfo = this.methodDefinition.DeclaringType.Resolve().GetRuntimeType().GetMethod("GeometryShader");
            }
            else
            {
                methodInfo = this.methodDefinition.GetMethodInfo();
            }

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
                    var attributes = parameterInfo.GetCustomAttributes(true)
                        .Where(x => x is AttributeAttribute || x is InputModeAttribute || x is OutputModeAttribute)
                        .ToList();
                    if (attributes.Count >= 1)
                    {
                        Debug.Assert(attributes.Count == 1);
                        var attribute = attributes[0] as Attribute;
                        switch (attribute)
                        {
                            case AttributeAttribute aa:
                                attributeDefinitions.WriteLine(Format("layout (location = {0}) in {1} {2};", (uint)aa.type, elementType, parameterInfo.Name));
                                break;
                            case InputModeAttribute ima:
                                var inputMode = new Dictionary<InputMode, string> {
                                    { InputMode.Points, "points" },
                                    { InputMode.Lines, "lines" },
                                    { InputMode.Triangles, "triangles" },
                                }[ima.mode];
                                attributeDefinitions.WriteLine(Format("layout ({0}) in;", inputMode));
                                break;
                            case OutputModeAttribute oma:
                                var outputMode = new Dictionary<OutputMode, string> {
                                    { OutputMode.Points, "points" },
                                    { OutputMode.LineStrip, "line_strip" },
                                    { OutputMode.TriangleStrip, "triangle_strip" },
                                }[oma.mode];
                                attributeDefinitions.WriteLine(Format("layout ({0}, max_vertices = {1}) out;", outputMode, oma.capacity));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        attributeDefinitions.WriteLine(Format("in {0} {1};", elementType, parameterInfo.Name));
                    }
                }
                else
                {
                    attributeDefinitions.WriteLine(Format("out {0} {1};", elementType, parameterInfo.Name));
                }
            }
            return attributeDefinitions.ToString();
        }

        public string GetMethodSourceCode(string methodName)
        {
            var code = new StringWriter();
            string parameters = "";
            if (methodName != "main")
            {
                parameters = Join(
                    ", ",
                    this.methodDefinition.Parameters
                        .Where(x => !x.IsOut)
                        .Select((parameter) => Format("{0} {1}", parameter.ParameterType, parameter.Name))
                );
            }
            code.WriteLine(Format(
                "{0} {1}({2}) {{",
                methodDefinition.ReturnType,
                methodName,
                parameters
            ));
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

        internal MethodDefinition methodDefinition { get; private set; }

        private Instruction first { get; set; }

        private Instruction last { get; set; }

        public CompileContext(MethodReference methodReference)
        {
            methodDefinition = methodReference.Resolve();
            declaringType = methodDefinition.DeclaringType;
            var instructions = methodDefinition.Body.Instructions;
            first = instructions.First();
            last = instructions.Last();
        }

        public CompileContext(MethodReference methodReference, Instruction first, Instruction last)
        {
            methodDefinition = methodReference.Resolve();
            declaringType = methodDefinition.DeclaringType;
            this.first = first;
            this.last = last;
        }

        private string GenerateInternal(Instruction first, Instruction last, TextWriter specific, ref EvaluationStack unhandled)
        {
            int evaluationStackPreviousCount = evaluationStack.Count;
            var writer = this.writer;
            this.writer = specific;
            for (var instruction = first; instruction != last.Next;)
            {
                try
                {
                    instruction = instruction.Step(last, this);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    Debug.LogFormat("Inner exception \"{0}\" occured in {1}, when generating the following codes:", e.InnerException, instruction);
                    var f = first;
                    while (f.Previous != null) { f = f.Previous; }
                    var l = last;
                    while (l.Next != null) { l = l.Next; }
                    for (var i = f; i != l.Next; i = i.Next)
                    {
                        Console.WriteLine(i);
                    }
                    throw new NotImplementedException();
                }
            }
            this.writer = writer;
            int evaluationStackCount = evaluationStack.Count;
            if (evaluationStackCount > evaluationStackPreviousCount)
            {
                int count = evaluationStackCount - evaluationStackPreviousCount;
                unhandled = new EvaluationStack();
                Stack<Variable> tmp = new Stack<Variable>();
                while ((count--) != 0)
                {
                    tmp.Push(evaluationStack.Pop());
                }
                while (tmp.Count != 0)
                {
                    unhandled.Push(tmp.Pop());
                }
            }
            else
            {
                unhandled = null;
            }
            return specific.ToString();
        }

        internal string GenerateWithoutWriting(Instruction first, Instruction last, ref EvaluationStack unhandled)
        {
            using (TextWriter writer = new StringWriter())
            {
                return GenerateInternal(first, last, writer, ref unhandled);
            }
        }

        static Dictionary<OpCode, LocalVariableOpCodeData> LocalVariableOpCodeDatas = new Dictionary<OpCode, LocalVariableOpCodeData>
        {
            {OpCodes.Stloc_0, new LocalVariableOpCodeData(OpCodes.Stloc_0, new OpCode[]{ OpCodes.Ldloc_0, OpCodes.Ldloca, OpCodes.Ldloca_S }, 0)},
            {OpCodes.Stloc_1, new LocalVariableOpCodeData(OpCodes.Stloc_1, new OpCode[]{ OpCodes.Ldloc_1, OpCodes.Ldloca, OpCodes.Ldloca_S }, 1)},
            {OpCodes.Stloc_2, new LocalVariableOpCodeData(OpCodes.Stloc_2, new OpCode[]{ OpCodes.Ldloc_2, OpCodes.Ldloca, OpCodes.Ldloca_S }, 2)},
            {OpCodes.Stloc_3, new LocalVariableOpCodeData(OpCodes.Stloc_3, new OpCode[]{ OpCodes.Ldloc_3, OpCodes.Ldloca, OpCodes.Ldloca_S }, 3)},
            {OpCodes.Stloc_S, new LocalVariableOpCodeData(OpCodes.Stloc_S, new OpCode[]{ OpCodes.Ldloc_S, OpCodes.Ldloca, OpCodes.Ldloca_S }, null)},
        };

        static OpCode[] LocalVariableSetOpCodes = new OpCode[]
        {
            OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Stloc_S,
        };

        static OpCode[] LocalVariableOpCodes = new OpCode[]
        {
            OpCodes.Stloc_0, OpCodes.Ldloc_0,
            OpCodes.Stloc_1, OpCodes.Ldloc_1,
            OpCodes.Stloc_2, OpCodes.Ldloc_2,
            OpCodes.Stloc_3, OpCodes.Ldloc_3,
            OpCodes.Stloc_S, OpCodes.Ldloc_S,
            OpCodes.Ldloca, OpCodes.Ldloca_S
        };

        private HashSet<uint> unnecessaryLocalVariableIDs = new HashSet<uint>();

        public bool IsUnnecessaryLocalVariableID(uint id)
        {
            return unnecessaryLocalVariableIDs.Contains(id);
        }

        public uint? GetLocalVariableInstructionIndex(Instruction instruction)
        {
            if (!LocalVariableOpCodes.Contains(instruction.OpCode))
            {
                return null;
            }
            else if (instruction.OpCode == OpCodes.Stloc_0 ||
                    instruction.OpCode == OpCodes.Ldloc_0)
            {
                return 0;
            }
            else if (instruction.OpCode == OpCodes.Stloc_1 ||
                instruction.OpCode == OpCodes.Ldloc_1)
            {
                return 1;
            }
            else if (instruction.OpCode == OpCodes.Stloc_2 ||
                instruction.OpCode == OpCodes.Ldloc_2)
            {
                return 2;
            }
            else if (instruction.OpCode == OpCodes.Stloc_3 ||
                instruction.OpCode == OpCodes.Ldloc_3)
            {
                return 3;
            }
            else if (instruction.OpCode == OpCodes.Stloc_S ||
                instruction.OpCode == OpCodes.Ldloc_S ||
                instruction.OpCode == OpCodes.Ldloca ||
                instruction.OpCode == OpCodes.Ldloca_S)
            {
                return (uint)((instruction.Operand as VariableDefinition).Index);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private HashSet<uint> ScanUnnecessaryLocalVariableIDs()
        {
            HashSet<uint> unnecessaryLocalVariableIDs = new HashSet<uint>();
            HashSet<uint> failedLocalVariableIDs = new HashSet<uint>();
            for (var instruction = this.first; instruction != this.last.Next; instruction = instruction.Next)
            {
                uint? index_ = GetLocalVariableInstructionIndex(instruction);
                if (index_ == null)
                {
                    continue;
                }
                uint index = index_.Value;
                if (failedLocalVariableIDs.Contains(index) ||
                    instruction.Next == null ||
                    !LocalVariableSetOpCodes.Contains(instruction.OpCode) ||
                    unnecessaryLocalVariableIDs.Contains(index))
                {
                    if (unnecessaryLocalVariableIDs.Contains(index))
                    {
                        unnecessaryLocalVariableIDs.Remove(index);
                    }
                    failedLocalVariableIDs.Add(index);
                    continue;
                }
                var next = instruction.Next;
                bool found = false;
                foreach (var data in LocalVariableOpCodeDatas.Values)
                {
                    if (data.setOpCode == instruction.OpCode && data.getOpCodes.Contains(next.OpCode))
                    {
                        unnecessaryLocalVariableIDs.Add(index);
                        instruction = instruction.Next;
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    continue;
                }
                if (unnecessaryLocalVariableIDs.Contains(index))
                {
                    unnecessaryLocalVariableIDs.Remove(index);
                }
                failedLocalVariableIDs.Add(index);
            }
            return unnecessaryLocalVariableIDs;
        }

        public string Generate()
        {
            EvaluationStack unhandled = null;
#if false  // DEBUG
            for (var instruction = first; instruction != last.Next; instruction = instruction.Next)
            {
                Console.WriteLine(instruction);
            }
#endif
            unnecessaryLocalVariableIDs = ScanUnnecessaryLocalVariableIDs();
            return GenerateInternal(this.first, this.last, this.writer, ref unhandled);
        }

        public Variable Allocate(Variable existed, uint index)
        {
            return allocator.Allocate(existed, index);
        }

        public Variable Allocate(TypeReference type, uint index)
        {
            return allocator.Allocate(type, index);
        }

        public Variable GetLocal(uint index)
        {
            return allocator.Get(index);
        }

        public Variable Push(TypeReference type, object name)
        {
            return evaluationStack.Push(type, name);
        }

        public Variable Push(Variable value)
        {
            evaluationStack.Push(value);
            return value;
        }

        public Variable Push(ParameterReference parameterReference)
        {
            return evaluationStack.Push(parameterReference);
        }

        public Variable Push(PropertyReference propertyReference)
        {
            return evaluationStack.Push(propertyReference);
        }

        public List<Variable> Pop(int count)
        {
            return evaluationStack.Pop(count);
        }

        public Variable Pop()
        {
            return evaluationStack.Pop();
        }

        public Variable Peek()
        {
            return evaluationStack.Peek();
        }

        public CodeBlock GetPeekCodeBlock()
        {
            return codeBlockStack.Peek();
        }

        public CodeBlockType GetPeekCodeBlockType()
        {
            return GetPeekCodeBlock().type;
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
                if (arg == null)
                {
                    parameters.Add(null);
                }
                else if(arg is TypeReference)
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
                    var value = arg as Value;
                    if (value.type.FullName.StartsWith("Saffiano.Input`1"))
                    {
                        parameters.Add("gl_in");
                    }
                    else
                    {
                        parameters.Add(value.ToString());
                    }
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

        public Value Property(Value @this, PropertyReference propertyReference)
        {
            var propertyDefinition = propertyReference.Resolve();
            var methodDefinition = propertyDefinition.GetMethod;
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
            string s = string.Format(pattern, @this.name, propertyReference.Name);
            return new Value(propertyDefinition.PropertyType, s);
        }

        public Variable Field(Variable @this, FieldDefinition fieldDefinition)
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
            if (type.IsEnum)
            {
                return "int";
            }
            else if(type.IsValueType)
            {
                if (type == typeof(bool))  return "bool" ;
                if (type == typeof(int))   return "int";
                if (type == typeof(float)) return "float";
                if (type == typeof(void))  return "void";
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

        public Value Method(MethodReference methodReference, Variable[] parameters)
        {
            // built-in shader method
            MethodBase methodInfo = null;
            if (methodReference.HasThis)
            {
                var @this = parameters[0];
                methodInfo = methodReference.GetMethodInfoWithGenericInstanceType(@this.type as GenericInstanceType);
            }
            else
            {
                methodInfo = methodReference.GetMethodInfo();
            }
            var shaderAttributes = methodInfo.GetCustomAttributes<ShaderAttribute>();
            string pattern = null;
            if (shaderAttributes.Any())
            {
                pattern = shaderAttributes.First().pattern;
            }
            var md = methodReference.Resolve();
            TypeReference returnType = md.ReturnType;
            if (md.IsConstructor)
            {
                var parameterPatterns = new List<string>();
                for (int i = 0; i < md.Parameters.Count; i++)
                {
                    parameterPatterns.Add(string.Format("{{{0}}}", i));
                }
                pattern = Format("{0}({1})", md.DeclaringType, Join(", ", parameterPatterns));
                returnType = md.DeclaringType;
            }
            if (pattern != null)
            {
                return new Value(returnType, Format(pattern, parameters));
            }
            var callerDeclaringType = methodDefinition.DeclaringType;
            var calleeDeclaringType = methodReference.DeclaringType.Resolve();
            if (calleeDeclaringType.IsBaseOf(callerDeclaringType) || calleeDeclaringType == callerDeclaringType)
            {
                var calleeMethodDefinition = methodReference.Resolve();
                string methodName = Format("{0}_{1}", callerDeclaringType.Name, calleeMethodDefinition.Name);
                methods[methodName] = calleeMethodDefinition;
                var finalParameters = new List<Variable>();
                for (int index = 0; index < parameters.Length; index++)
                {
                    if (calleeMethodDefinition.HasThis && index == 0)
                    {
                        continue;
                    }
                    bool isShaderEntrance = typeof(ShaderType)
                        .GetEnumValues()
                        .Cast<ShaderType>()
                        .Select(x => x.ToString())
                        .Contains(calleeMethodDefinition.Name);
                    if (calleeMethodDefinition.Parameters[index - (calleeMethodDefinition.HasThis ? 1 : 0)].IsOut)
                    {
                        if (!isShaderEntrance)
                        {
                            throw new Exception("Out parameter can be used only in shader entrace method.");
                        }
                        continue;
                    }
                    finalParameters.Add(parameters[index]);
                }
                return new Value(returnType, Format("{0}({1})", methodName, Join(", ", finalParameters)));
            }
            if (methodInfo.DeclaringType.FullName.StartsWith("Saffiano.Output`1"))
            {
                var output = parameters[0];
                var array = parameters[1] as Array;
                StringWriter _writer = new StringWriter();
                foreach (VertexValue item in array)
                {
                    _writer.WriteLine(Format("gl_Position = {0}; EmitVertex();", item.gl_Position));
                }
                _writer.WriteLine("EndPrimitive();");
                return new Value(returnType, _writer.ToString());
            }
            Debug.LogErrorFormat("Method \"{0}.{1}\" is not a built-in shader method.", methodInfo.DeclaringType.FullName, methodInfo.Name);
            return null;
        }

        public Value Method(MethodReference methodReference, List<Variable> parameters)
        {
            return Method(methodReference, parameters.ToArray());
        }

        public void Assign(Variable target, object value)
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

        public void WriteLine(string content)
        {
            writer.WriteLine(content);
        }

        public Value ConstructVertex(MethodReference methodReference, List<Variable> parameters)
        {
            var type = methodReference.DeclaringType.Resolve().GetRuntimeType();
            Debug.Assert(typeof(Vertex).IsAssignableFrom(type));
            MethodDefinition methodDefinition = methodReference.Resolve();
            var instructions = methodDefinition.Body.Instructions;
            var vertexTypeDefinition = typeof(Vertex).GetTypeDefinition();
            return new VertexValue(vertexTypeDefinition, null, parameters[0]);
        }
    }
}
