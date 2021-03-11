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

        public static bool Call(MethodReference methodReference, CompileContext compileContext)
        {
            Value @internal = null;
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
                    @internal = compileContext.AllocateInternal(propertyDefinition.PropertyType);
                    compileContext.Assign(@internal, compileContext.Property(@this, propertyDefinition));
                    compileContext.Push(@internal);
                }
            }
            else  // method
            {
                var parameters = compileContext.Pop(methodReference.Parameters.Count());
                if (methodDefinition.IsConstructor)
                {
                    // Call the initializer on the local (from ECMA-335: Page 163)
                    @internal = compileContext.AllocateInternal(methodDefinition.DeclaringType);
                    var method = compileContext.Method(methodDefinition, parameters.ToArray());
                    compileContext.Assign(@internal, method);
                    if (compileContext.Peek().isAddress)
                    {
                        var target = compileContext.Pop();
                        compileContext.Assign(target, @internal);
                    }
                }
                else
                {
                    if (methodDefinition.HasThis)
                    {
                        parameters.Insert(0, compileContext.Pop());
                    }
                    @internal = compileContext.AllocateInternal(methodDefinition.ReturnType);
                    var method = compileContext.Method(methodDefinition, parameters.ToArray());
                    compileContext.Assign(@internal, method);
                    compileContext.Push(@internal);
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
            compileContext.Push(typeof(float).GetTypeDefinition(), (float)instruction.Operand);
            return true;
        }

        public static bool Construct(MethodReference methodReference, CompileContext compileContext)
        {
            var @internal = compileContext.AllocateInternal(methodReference.DeclaringType);
            var parameters = compileContext.Pop(methodReference.Parameters.Count);
            var method = compileContext.Method(methodReference, parameters);
            compileContext.Assign(@internal, method);
            compileContext.Push(@internal);
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
            return true;
        }

        public static bool Stloc_N(Instruction instruction, CompileContext compileContext, uint index)
        {
            var value = compileContext.Pop();
            var local = compileContext.AllocateLocal(value.type, index);
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
            var local = compileContext.AllocateLocal(variableDefinition.VariableType, (uint)variableDefinition.Index);
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
            var local = compileContext.AllocateLocal(variableDefinition.VariableType, (uint)variableDefinition.Index);
            local.isAddress = true;
            compileContext.Push(local);
            return true;
        }

        [Instruction(Mono.Cecil.Cil.Code.Ldfld)]
        public static bool Ldfld(Instruction instruction, CompileContext compileContext)
        {
            // ldfld field - Push the value of field of object (or value type) obj, onto the stack.
            var fieldDefinition = instruction.Operand as FieldDefinition;
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

        public static bool Operator(Instruction instruction, CompileContext compileContext, string @operator)
        {
            var b = compileContext.Pop();
            var a = compileContext.Pop();
            var @internal = compileContext.AllocateInternal(a.type);
            compileContext.Assign(@internal, compileContext.Format("{0} {1} {2}", a, @operator, b));
            compileContext.Push(@internal);
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

        public static bool Step(this Instruction instruction, CompileContext compileContext)
        {
            var code = instruction.OpCode.Code;
            MethodInfo method = null;
            if (!methods.TryGetValue(code, out method))
            {
                throw new Exception(string.Format("Unsupported operate code: {0}", code));
            }
            return (bool)method.Invoke(null, new object[] { instruction, compileContext });
        }
    }
}
