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
                instruction = instruction.Step(this);
            }
            this.writer = writer;
            int evaluationStackCount = evaluationStack.Count;
            Debug.Assert(evaluationStackCount >= evaluationStackPreviousCount);
            int count = evaluationStackCount - evaluationStackPreviousCount;
            if (count != 0)
            {
                unhandled = new EvaluationStack();
                Stack<Value> tmp = new Stack<Value>();
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

        public Value Allocate(TypeReference type, uint index)
        {
            return allocator.Allocate(type, index);
        }

        public Value GetLocal(uint index)
        {
            return allocator.Get(index);
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

        public void Begin(CodeBlockType blockType, Instruction first, Instruction last)
        {
            codeBlockStack.Push(new CodeBlock(first, last, blockType));
            writer.WriteLine("{");
        }

        public bool TryEnd(Instruction current)
        {
            if (codeBlockStack.Count == 0)
            {
                return false;
            }
            if (GetPeekCodeBlock().last == current)
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

        public Value Method(MethodReference methodReference, Value[] parameters)
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
                string methodName = Format("{0}_{1}", callerDeclaringType.Name, methodReference.Resolve().Name);
                methods[methodName] = methodReference.Resolve();
                return new Value(typeof(void).GetTypeDefinition(), Format("{0}();", methodName));
            }
            Debug.LogErrorFormat("Method \"{0}.{1}\" is not a built-in shader method.", methodInfo.DeclaringType.FullName, methodInfo.Name);
            return null;
        }

        public Value Method(MethodReference methodReference, List<Value> parameters)
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
            writer.WriteLine("if({0} != 0)", value);
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
