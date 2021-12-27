using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saffiano.ShaderCompilation
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class InstructionAttribute : Attribute
    {
        public Mono.Cecil.Cil.Code code { get; private set; }

        public InstructionAttribute(Mono.Cecil.Cil.Code code)
        {
            this.code = code;
        }
    }

    internal static class InstructionExtensions
    {
        private static Dictionary<Mono.Cecil.Cil.Code, MethodInfo> methods = null;

        static InstructionExtensions()
        {
            methods = new Dictionary<Code, MethodInfo>();
            var all = typeof(InstructionExtensions).GetMethods();
            foreach (var method in all)
            {
                var attributes = method.GetCustomAttributes<InstructionAttribute>();
                foreach (var attribute in attributes)
                {
                    methods.Add(attribute.code, method);
                }
            }
        }

        [Instruction(Mono.Cecil.Cil.Code.Nop)]
        public static bool Nop(Instruction instruction, CompileContext compileContext)
        {
            // nop – no operation.
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldobj)]
        public static bool Ldobj(Instruction instruction, CompileContext compileContext)
        {
            // ldobj – copy a value from an address to the stack
            var value = compileContext.Pop();
            compileContext.Push(value);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarg_S)]
        public static bool Ldarg_S(Instruction instruction, CompileContext compileContext)
        {
            // ldarg.s num - Load argument numbered num onto the stack, short form.
            compileContext.Push(instruction.Operand as ParameterDefinition);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarg_0)]
        public static bool Ldarg_0(Instruction instruction, CompileContext compileContext)
        {
            // ldarg.s num - Load argument numbered num onto the stack, short form.
            compileContext.Push(compileContext.declaringType, "this");
            return true;
        }

        public static bool Ldarg_N(Instruction instruction, CompileContext compileContext, int n)
        {
            compileContext.Push(compileContext.parameters[n - 1]);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarg_1)]
        public static bool Ldarg_1(Instruction instruction, CompileContext compileContext)
        {
            // ldarg.1 - Load argument 1 onto the stack.
            Ldarg_N(instruction, compileContext, 1);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarg_2)]
        public static bool Ldarg_2(Instruction instruction, CompileContext compileContext)
        {
            // ldarg.2 - Load argument 2 onto the stack.
            Ldarg_N(instruction, compileContext, 2);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarg_3)]
        public static bool Ldarg_3(Instruction instruction, CompileContext compileContext)
        {
            // ldarg.3 - Load argument 3 onto the stack.
            Ldarg_N(instruction, compileContext, 3);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldarga_S)]
        public static bool Ldarga_S(Instruction instruction, CompileContext compileContext)
        {
            // ldloca.s indx - Load address of local variable with index indx, short form.
            var parameterDefinition = instruction.Operand as ParameterDefinition;
            var local = compileContext.Push(parameterDefinition);
            local.isAddress = true;
            return true;
        }

        public static bool Call(MethodReference methodReference, CompileContext compileContext)
        {
            var methodDefinition = methodReference.Resolve();
            if (methodDefinition.IsGetter)  // call property getter
            {
                var propertyName = methodDefinition.Name.Substring("get_".Length);
                var @this = compileContext.Pop();
                var typeDefinition = @this.type.Resolve();
                var propertyDefinition = typeDefinition.FindPropertyDefinitionIncludeAncestors(propertyName);
                if (typeof(ScriptableMaterial).IsAssignableFrom(typeDefinition.GetRuntimeType()))
                {
                    Debug.Assert((@this.name as string) == "this");
                    // uniform type is defined as a property in ScriptingMaterial
                    compileContext.Push(propertyDefinition);
                    var propertyInfo = propertyDefinition.DeclaringType.GetRuntimeType().GetProperty(propertyDefinition.Name);
                    if (propertyInfo.GetCustomAttribute<UniformAttribute>() != null)
                    {
                        compileContext.AddUniform(new Uniform(propertyInfo));
                    }
                    else
                    {
                        throw new Exception("uncertain material property get behaviour");
                    }
                    return true;
                }
                else
                {
                    compileContext.Push(compileContext.Property(@this, propertyDefinition));
                }
            }
            else  // method
            {
                var parameters = compileContext.Pop(methodReference.Parameters.Count());
                if (methodDefinition.IsConstructor)
                {
                    // Call the initializer on the local (from ECMA-335: Page 163)
                    var method = compileContext.Method(methodDefinition, parameters.ToArray());
                    if (compileContext.Peek().isAddress)
                    {
                        var target = compileContext.Pop();
                        compileContext.Assign(target, method);
                    }
                }
                else
                {
                    if (methodDefinition.HasThis)
                    {
                        parameters.Insert(0, compileContext.Pop());
                    }
                    var method = compileContext.Method(methodDefinition, parameters.ToArray());
                    if (!methodDefinition.ReturnType.Resolve().IsSameRuntimeOf(typeof(void).GetTypeDefinition()))
                    {
                        compileContext.Push(method);
                    }
                    else
                    {
                        compileContext.WriteLine(method.ToString());
                    }
                }
            }
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Call)]
        public static bool Call(Instruction instruction, CompileContext compileContext)
        {
            // call – call a method.
            return Call(instruction.Operand as MethodReference, compileContext);
        }

        [Instruction(Mono.Cecil.Cil.Code.Callvirt)]
        public static bool Callvirt(Instruction instruction, CompileContext compileContext)
        {
            // callvirt method - Call a method associated with an object.
            return Call(instruction.Operand as MethodReference, compileContext);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_R4)]
        public static bool Ldc_R4(Instruction instruction, CompileContext compileContext)
        {
            // ldc.r4 num - Push num of type float32 onto the stack as F.
            compileContext.Push(typeof(float).GetTypeDefinition(), string.Format("float({0})", (float)instruction.Operand));
            return true;
        }

        public static bool Ldc_I4_X(Instruction instruction, CompileContext compileContext, int value)
        {
            // ldc.i4.m1 - Push -1 onto the stack as int32.
            compileContext.Push(typeof(int).GetTypeDefinition(), value);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_0)]
        public static bool Ldc_I4_0(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 1 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 0);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_1)]
        public static bool Ldc_I4_1(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 1 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 1);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_2)]
        public static bool Ldc_I4_2(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 2 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 2);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_3)]
        public static bool Ldc_I4_3(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 3 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 3);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_4)]
        public static bool Ldc_I4_4(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 4 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 4);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_5)]
        public static bool Ldc_I4_5(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 5 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 5);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_6)]
        public static bool Ldc_I4_6(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 6 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 6);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_7)]
        public static bool Ldc_I4_7(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 7 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 7);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_8)]
        public static bool Ldc_I4_8(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 8 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, 8);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_S)]
        public static bool Ldc_I4_S(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.1 - Push 1 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, Convert.ToInt32((sbyte)instruction.Operand));
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldc_I4_M1)]
        public static bool Ldc_I4_M1(Instruction instruction, CompileContext compileContext)
        {
            // ldc.i4.m1 - Push -1 onto the stack as int32.
            return Ldc_I4_X(instruction, compileContext, -1);
        }

        public static bool Construct(MethodReference methodReference, CompileContext compileContext)
        {
            var parameters = compileContext.Pop(methodReference.Parameters.Count);
            var method = compileContext.Method(methodReference, parameters);
            compileContext.Push(method);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Newobj)]
        public static bool Newobj(Instruction instruction, CompileContext compileContext)
        {
            // newobj ctor - Allocate an uninitialized object or value type and call ctor.
            return Construct(instruction.Operand as MethodReference, compileContext);
        }

        [Instruction(Mono.Cecil.Cil.Code.Stobj)]
        public static bool Stobj(Instruction instruction, CompileContext compileContext)
        {
            // stobj typeTok - Store a value of type typeTok at an address.
            var value = compileContext.Pop();
            var target = compileContext.Pop();
            compileContext.Assign(target, value);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ret)]
        public static bool Ret(Instruction instruction, CompileContext compileContext)
        {
            // ret - Return from method, possibly with a value.
            if (instruction.Operand != null)
            {
                throw new NotImplementedException();
            }
            compileContext.WriteLine("return;");
            return true;
        }

        public static bool Stloc_N(Instruction instruction, CompileContext compileContext, uint index)
        {
            var value = compileContext.Pop();
            var local = compileContext.Allocate(value.type, index);
            compileContext.Assign(local, value);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Stloc_0)]
        public static bool Stloc_0(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.0 - Load local variable 0 onto stack.
            return Stloc_N(instruction, compileContext, 0);
        }

        [Instruction(Mono.Cecil.Cil.Code.Stloc_1)]
        public static bool Stloc_1(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.1 - Load local variable 1 onto stack.
            return Stloc_N(instruction, compileContext, 1);
        }

        [Instruction(Mono.Cecil.Cil.Code.Stloc_2)]
        public static bool Stloc_2(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.2 - Load local variable 2 onto stack.
            return Stloc_N(instruction, compileContext, 2);
        }

        [Instruction(Mono.Cecil.Cil.Code.Stloc_3)]
        public static bool Stloc_3(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.3 - Load local variable 3 onto stack.
            return Stloc_N(instruction, compileContext, 3);
        }

        [Instruction(Mono.Cecil.Cil.Code.Stloc_S)]
        public static bool Stloc_S(Instruction instruction, CompileContext compileContext)
        {
            // stloc.s - Pop a value from stack into local variable.
            return Stloc_N(instruction, compileContext, (uint)((instruction.Operand as VariableDefinition).Index));
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloc_S)]
        public static bool Ldloc_S(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.s indx - Load local variable of index indx onto stack, short form.
            var variableDefinition = instruction.Operand as VariableDefinition;
            var local = compileContext.Allocate(variableDefinition.VariableType, (uint)variableDefinition.Index);
            compileContext.Push(local);
            return true;
        }

        public static bool Ldloc_N(Instruction instruction, CompileContext compileContext, uint index)
        {
            compileContext.Push(compileContext.GetLocal(index));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloc_0)]
        public static bool Ldloc_0(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.0 - Load local variable 0 onto stack.
            return Ldloc_N(instruction, compileContext, 0);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloc_1)]
        public static bool Ldloc_1(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.1 - Load local variable 1 onto stack.
            return Ldloc_N(instruction, compileContext, 1);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloc_2)]
        public static bool Ldloc_2(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.2 - Load local variable 2 onto stack.
            return Ldloc_N(instruction, compileContext, 2);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloc_3)]
        public static bool Ldloc_3(Instruction instruction, CompileContext compileContext)
        {
            // ldloc.3 - Load local variable 3 onto stack.
            return Ldloc_N(instruction, compileContext, 3);
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldloca_S)]
        public static bool Ldloca_S(Instruction instruction, CompileContext compileContext)
        {
            // ldloca.s indx - Load address of local variable with index indx, short form.
            var variableDefinition = instruction.Operand as VariableDefinition;
            var local = compileContext.Allocate(variableDefinition.VariableType, (uint)variableDefinition.Index);
            local.isAddress = true;
            compileContext.Push(local);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldfld)]
        public static bool Ldfld(Instruction instruction, CompileContext compileContext)
        {
            // ldfld field - Push the value of field of object (or value type) obj, onto the stack.
            var fieldDefinition = (instruction.Operand as FieldReference).Resolve();
            var @this = compileContext.Pop();
            compileContext.Push(fieldDefinition.FieldType, compileContext.Field(@this, fieldDefinition));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Stfld)]
        public static bool Stfld(Instruction instruction, CompileContext compileContext)
        {
            // stfld field - Replace the value of field of the object obj with value
            var fieldDefinition = instruction.Operand as FieldDefinition;
            var value = compileContext.Pop();
            var @this = compileContext.Pop();
            compileContext.Assign(compileContext.Field(@this, fieldDefinition), value);
            return true;
        }

        public static bool Operator(Instruction instruction, CompileContext compileContext, string @operator, TypeReference type = null)
        {
            var b = compileContext.Pop();
            var a = compileContext.Pop();
            string pattern;
            if (type != null)
            {
                pattern = "{3}({0} {1} {2})";
            }
            else
            {
                pattern = "({0} {1} {2})";
            }
            compileContext.Push(new Value(type != null ? type : a.type, CompileContext.Format(pattern, a, @operator, b, type)));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Add)]
        public static bool Add(Instruction instruction, CompileContext compileContext)
        {
            Operator(instruction, compileContext, "+");
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Sub)]
        public static bool Sub(Instruction instruction, CompileContext compileContext)
        {
            Operator(instruction, compileContext, "-");
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Mul)]
        public static bool Mul(Instruction instruction, CompileContext compileContext)
        {
            Operator(instruction, compileContext, "*");
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Div)]
        public static bool Div(Instruction instruction, CompileContext compileContext)
        {
            Operator(instruction, compileContext, "/");
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Rem)]
        public static bool Rem(Instruction instruction, CompileContext compileContext)
        {
            var b = compileContext.Pop();
            var a = compileContext.Pop();
            compileContext.Push(new Value(a.type, CompileContext.Format("mod({0}, {1})", a, b)));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Stind_R4)]
        public static bool Stind_R4(Instruction instruction, CompileContext compileContext)
        {
            var value = compileContext.Pop();
            var target = compileContext.Pop();
            compileContext.Assign(target, value);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Neg)]
        public static bool Neg(Instruction instruction, CompileContext compileContext)
        {
            var value = compileContext.Pop();
            compileContext.Push(value.type, CompileContext.Format("-({0})", value));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Cgt)]
        public static bool Cgt(Instruction instruction, CompileContext compileContext)
        {
            // cgt - Push 1 (of type int32) if value1 > value2, else push 0.
            Operator(instruction, compileContext, ">", typeof(int).GetTypeDefinition());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Cgt_Un)]
        public static bool Cgt_Un(Instruction instruction, CompileContext compileContext)
        {
            // cgt.un –  Push 1 (of type int32) if value1 < value2, else push 0.
            Operator(instruction, compileContext, ">", typeof(int).GetTypeDefinition());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Clt)]
        public static bool Clt(Instruction instruction, CompileContext compileContext)
        {
            // clt –  Push 1 (of type int32) if value1 < value2, else push 0.
            Operator(instruction, compileContext, "<", typeof(int).GetTypeDefinition());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Clt_Un)]
        public static bool Clt_Un(Instruction instruction, CompileContext compileContext)
        {
            // clt.un –  Push 1 (of type int32) if value1 < value2, else push 0.
            Operator(instruction, compileContext, "<", typeof(int).GetTypeDefinition());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ceq)]
        public static bool Ceq(Instruction instruction, CompileContext compileContext)
        {
            // ceq - Push 1 (of type int32) if value1 > value2, else push 0.
            Operator(instruction, compileContext, "==", typeof(int).GetTypeDefinition());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Brtrue)]
        public static bool Brtrue(Instruction instruction, CompileContext compileContext)
        {
            // brtrue target - Branch to target if value is non-zero (true).
            compileContext.Push(typeof(int).GetTypeDefinition(), CompileContext.Format("int({0} == 1)", compileContext.Pop()));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Brfalse_S)]
        public static bool Brfalse_S(Instruction instruction, CompileContext compileContext)
        {
            // brfalse.s target - Branch to target if value is zero (false), short form.
            compileContext.Push(typeof(int).GetTypeDefinition(), CompileContext.Format("int({0} == 0)", compileContext.Pop()));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ble)]
        public static bool Ble(Instruction instruction, CompileContext compileContext)
        {
            // ble target - Branch to target if less than or equal to.
            var b = compileContext.Pop();
            var a = compileContext.Pop();
            compileContext.Push(typeof(int).GetTypeDefinition(), CompileContext.Format("int({0} <= {1})", a, b));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Bgt_Un_S)]
        public static bool Bgt_Un_S(Instruction instruction, CompileContext compileContext)
        {
            // bgt.un.s target - Branch to target if greater than (unsigned or unordered), short form.
            var b = compileContext.Pop();
            var a = compileContext.Pop();
            compileContext.Push(typeof(int).GetTypeDefinition(), CompileContext.Format("int({0} > {1})", a, b));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Conv_I4)]
        public static bool Conv_I4(Instruction instruction, CompileContext compileContext)
        {
            compileContext.Push(typeof(int).GetTypeDefinition(), CompileContext.Format("int({0})", compileContext.Pop()));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Conv_R4)]
        public static bool Conv_R4(Instruction instruction, CompileContext compileContext)
        {
            compileContext.Push(typeof(float).GetTypeDefinition(), CompileContext.Format("float({0})", compileContext.Pop()));
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Dup)]
        public static bool Dup(Instruction instruction, CompileContext compileContext)
        {
            compileContext.Push(compileContext.Peek());
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Pop)]
        public static bool Pop(Instruction instruction, CompileContext compileContext)
        {
            compileContext.Pop();
            return true;
        }

        public static Instruction Step(this Instruction current, Instruction last, CompileContext compileContext)
        {
            if (StatementStructure.Recognize(new CodeBlock(current, last), out StatementStructure statementStructure) != StatementStructureType.Unknown)
            {
                string s = statementStructure.Generate(compileContext);
                compileContext.WriteLine(s);
                return statementStructure.all.last.Next;
            }
            uint? localVariableIndex = compileContext.GetLocalVariableInstructionIndex(current);
            if (localVariableIndex != null && compileContext.IsUnnecessaryLocalVariableID(localVariableIndex.Value))
            {
                return current.Next.Next;
            }
            var code = current.OpCode.Code;
            if (!methods.TryGetValue(code, out MethodInfo method))
            {
                throw new Exception(string.Format("Unsupported operate code: {0}", code));
            }
            method.Invoke(null, new object[] { current, compileContext });
            return current.Next;
        }
    }
}
