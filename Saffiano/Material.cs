﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saffiano
{
    internal enum ShaderType
    {
        VertexShader = 0,
        FragmentShader = 35632,
    }

    public class UniformAttribute : Attribute
    {
    }

    public class AttributeAttribute : Attribute
    {
        public uint location { get; private set; }

        public AttributeAttribute(uint location)
        {
            this.location = location;
        }
    }

    public class Material
    {
        public GPUProgram shader { get; protected set; }

        protected Material()
        {
        }

        public Material(string vertexShaderFilePath, string fragmentShaderFilePath)
        {
            shader = GPUProgram.LoadFromFile(vertexShaderFilePath, fragmentShaderFilePath);
        }
    }

    internal static partial class Extension
    {
        public static TypeDefinition GetTypeDefinition(this Type type)
        {
            var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(type.Assembly.Location);
            return assemblyDefinition.MainModule.Types.First((td) => td.Name == type.Name);
        }

        public static Type GetRuntimeType(this TypeDefinition typeDefinition)
        {
            var assembly = AppDomain.CurrentDomain.Load(typeDefinition.Module.Assembly.FullName);
            return assembly.GetType(typeDefinition.FullName);
        }

        public static MethodReference FindMethod(this TypeDefinition typeDefinition, string methodName)
        {
            var list = typeDefinition.Methods.Where((md) => md.Name == methodName);
            return list.Any() ? list.First() : null;
        }

        public static MethodInfo GetMethodInfo(this MethodReference methodReference)
        {
            var methodDefinition = methodReference.Resolve();
            var typeDefinition = methodDefinition.DeclaringType;
            var declaringType = typeDefinition.GetRuntimeType();
            var parameterTypes = methodReference.Parameters.Select((pd) => {
                var td = pd.ParameterType.Resolve();
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
            return declaringType.GetMethod(methodReference.Name, all, null, parameterTypes, null);
        }
    }

    internal class IntermediateLanguageCompiler
    {
        private TextWriter writer = new StringWriter();

        class LocalVariable
        {
            public string name { get; private set; }

            public TypeReference type { get; private set; }

            public LocalVariable(string name, TypeReference type)
            {
                this.name = name;
                this.type = type;
            }
        }

        private Dictionary<uint, LocalVariable> localVariables = new Dictionary<uint, LocalVariable>();

        private LocalVariable GetLocalVariable(uint index, TypeReference type)
        {
            if (!localVariables.ContainsKey(index))
            {
                localVariables[index] = new LocalVariable(string.Format("{0}_{1}", type.FullName.Replace('.', '_'), index), type);
            }
            if (localVariables[index].type != type)
            {
                throw new Exception();
            }
            return localVariables[index];
        }

        private LocalVariable GetLocalVariable(uint index)
        {
            return localVariables[index];
        }

        class Element
        {
            public object @object { get; private set; }

            public TypeReference type { get; private set; }

            public Element(object @object) : this(@object, null)
            {
            }

            public Element(object @object, TypeReference type)
            {
                this.@object = @object;
                this.type = type;
            }

            public override string ToString()
            {
                return @object.ToString();
            }
        }

        private Stack<Element> stack = new Stack<Element>();

        private void Push(object @object, TypeReference type = null)
        {
            stack.Push(new Element(@object, type));
        }

        private Element Pop()
        {
            return stack.Pop();
        }

        private List<Element> Pop(int count)
        {
            List<Element> list = new List<Element>();
            for (int i = 0; i < count; i++)
            {
                list.Insert(0, Pop());
            }
            return list;
        }

        protected virtual string OnCompilingType(TypeReference typeReference)
        {
            // built-in shader type
            var type = typeReference.Resolve().GetRuntimeType();
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

        protected virtual string OnCompilingMethod(MethodReference methodReference)
        {
            // built-in shader method
            MethodInfo methodInfo = methodReference.GetMethodInfo();
            var shaderAttributes = methodInfo.GetCustomAttributes<ShaderAttribute>();
            if (shaderAttributes.Any())
            {
                return shaderAttributes.First().pattern;
            }
            Debug.LogErrorFormat("Method \"{0}\" is not a built-in shader method.", methodInfo.Name);
            return null;
        }

        protected virtual string OnCompilingObject(object @object)
        {
            // object
            return @object.ToString();
        }

        private string Format(string format, params object[] args)
        {
            List<object> list = new List<object>();
            foreach (var arg in args)
            {
                var @object = arg;
                if (@object is Element)
                {
                    @object = (arg as Element).@object;
                }
                if (@object is Type)
                {
                    list.Add(OnCompilingType(((Type)@object).GetTypeDefinition()));
                }
                else if (@object is TypeReference)
                {
                    list.Add(OnCompilingType((TypeReference)@object));
                }
                else
                {
                    list.Add(OnCompilingObject(@object));
                }
            }
            if (list.Count == 0)
            {
                return format;
            }
            else
            {
                return string.Format(format, list.ToArray());
            }
        }

        public String Join<T>(String separator, IEnumerable<T> values)
        {
            List<object> list = new List<object>();
            foreach (var value in values)
            {
                var @object = (object)value;
                if (@object is Element)
                {
                    @object = (value as Element).@object;
                }
                list.Add(OnCompilingObject(@object));
            }
            return string.Join(separator, list);
        }

        private void WriteLine(string content)
        {
            writer.WriteLine(content);
        }

        private bool MakeMethodCall(MethodReference methodReference)
        {
            var methodName = methodReference.Name;
            if (methodName.StartsWith("get_"))
            {
                var propertyName = methodName.Substring("get_".Length);
                var typeDefinition = Pop().@object as TypeDefinition;
                var propertyDefinition = typeDefinition.Properties.First((x) => x.Name == propertyName);
                if (propertyDefinition != null && methodReference.Parameters.Count() == 0 && methodReference.HasThis)
                {
                    // call property getter
                    Push(propertyDefinition.Name, propertyDefinition.PropertyType);
                    return true;
                }
            }
            Push(
                Format(OnCompilingMethod(methodReference), Pop(methodReference.Parameters.Count()).ToArray()),
                methodReference.ReturnType
            );
            return true;
        }

        private bool MakeParameterList(IEnumerable<ParameterDefinition> parameterDefinitions)
        {
            Push(Join(", ", Pop(parameterDefinitions.Count())));
            return true;
        }

        private bool MakeConstructorCall(MethodReference methodReference)
        {
            MakeParameterList(methodReference.Parameters);
            Push(
                Format("{0}({1})", methodReference.DeclaringType, Pop()),
                methodReference.ReturnType
            );
            return true;
        }

        private bool MakeSetObject()
        {
            var elements = Pop(2);
            Push(Join(" = ", elements), elements[0].type);
            return true;
        }

        private bool MakeReturn(object value)
        {
            if (value != null)
            {
                Push(Format("return {0}", Pop()));
            }
            return true;
        }

        public string Compile(MethodReference methodReference)
        {
            if (methodReference == null)
            {
                return null;
            }

            writer.Flush();

            var methodDefinition = methodReference.Resolve();
            var typeDefinition = methodDefinition.DeclaringType;
            var type = typeDefinition.GetRuntimeType();
            var methodInfo = methodDefinition.GetMethodInfo();

            WriteLine("#version 330 core");
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(UniformAttribute), true);
                if (attributes.Length == 0)
                {
                    continue;
                }
                WriteLine(Format("uniform {0} {1};", property.PropertyType, property.Name));
            }

            // Method parameters
            var parameterInfos = methodInfo.GetParameters();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.IsIn)
                {
                    var attributes = parameterInfo.GetCustomAttributes(typeof(AttributeAttribute), true);
                    if (attributes.Length == 1)
                    {
                        var attribute = attributes[0] as AttributeAttribute;
                        WriteLine(Format("layout (location = {0}) in {1} {2};", attribute.location, parameterInfo.ParameterType.GetElementType(), parameterInfo.Name));
                    }
                    else
                    {
                        WriteLine(Format("in {0} {1};", parameterInfo.ParameterType.GetElementType(), parameterInfo.Name));
                    }
                }
                if (parameterInfo.IsOut)
                {
                    WriteLine(Format("out {0} {1};", parameterInfo.ParameterType.GetElementType(), parameterInfo.Name));
                }
            }

            // Method body

            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Nop)
                {
                    // nop – no operation.
                    continue;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_S)
                {
                    // ldarg.s num - Load argument numbered num onto the stack, short form.
                    var parameterDefinition = instruction.Operand as ParameterDefinition;
                    Push(parameterDefinition, parameterDefinition.ParameterType);
                }
                else if (instruction.OpCode == OpCodes.Ldarg_0)
                {
                    // ldarg.0 - Load argument 0 onto the stack.
                    Push(typeDefinition, typeDefinition);
                }
                else if (instruction.OpCode == OpCodes.Ldarg_1)
                {
                    // ldarg.1 - Load argument 1 onto the stack.
                    Push(methodDefinition.Parameters[0], methodDefinition.Parameters[0].ParameterType);
                }
                else if (instruction.OpCode == OpCodes.Ldarg_2)
                {
                    // ldarg.2 - Load argument 2 onto the stack.
                    Push(methodDefinition.Parameters[1], methodDefinition.Parameters[1].ParameterType);
                }
                else if (instruction.OpCode == OpCodes.Ldarg_3)
                {
                    // ldarg.3 - Load argument 3 onto the stack.
                    Push(methodDefinition.Parameters[2], methodDefinition.Parameters[2].ParameterType);
                }
                else if (instruction.OpCode == OpCodes.Ldarg_S)
                {
                    // ldarg.s num - Load argument numbered num onto the stack, short form.
                    var pd = methodDefinition.Parameters.First((x) => x.Name == (instruction.Operand as ParameterDefinition).Name);
                    Push(pd, pd.ParameterType);
                }
                else if (instruction.OpCode == OpCodes.Call)
                {
                    // call – call a method.
                    if (!MakeMethodCall(instruction.Operand as MethodReference))
                    {
                        throw new NotImplementedException(string.Format("Unknown method call: {0}", instruction.Operand));
                    }
                }
                else if (instruction.OpCode == OpCodes.Ldobj)
                {
                    // ldobj typeTok - Copy the value stored at address src to the stack.
                    ;
                }
                else if (instruction.OpCode == OpCodes.Ldc_R4)
                {
                    // ldc.r4 num - Push num of type float32 onto the stack as F.
                    Push((float)instruction.Operand, typeof(float).GetTypeDefinition());
                }
                else if (instruction.OpCode == OpCodes.Newobj)
                {
                    // newobj ctor - Allocate an uninitialized object or value type and call ctor.
                    MakeConstructorCall(instruction.Operand as MethodReference);
                }
                else if (instruction.OpCode == OpCodes.Stobj)
                {
                    // stobj typeTok - Store a value of type typeTok at an address.
                    MakeSetObject();
                }
                else if (instruction.OpCode == OpCodes.Ret)
                {
                    // ret - Return from method, possibly with a value.
                    MakeReturn(instruction.Operand);
                }
                else if (instruction.OpCode == OpCodes.Stloc_0)
                {
                    // stloc.0 - Pop a value from stack into local variable 0.
                    var element = Pop();
                    var localVariable = GetLocalVariable(0, element.type);
                    Push(Format("{0} {1} = {2}", localVariable.type, localVariable.name, element.@object));
                }
                else if (instruction.OpCode == OpCodes.Ldloc_0)
                {
                    // ldloc.0 - Load local variable 0 onto stack.
                    var localVariable = GetLocalVariable(0);
                    Push(localVariable.name, localVariable.type);
                }
                else if (instruction.OpCode == OpCodes.Ldfld)
                {
                    // ldfld field - Push the value of field of object (or value type) obj, onto the stack.
                    var fd = instruction.Operand as FieldDefinition;
                    Push(Format("({0}).{1}", Pop(), fd.Name), fd.FieldType);
                }
                else if (instruction.OpCode == OpCodes.Sub)
                {
                    // sub - Subtract value2 from value1, returning a new value.
                    var value2 = Pop();
                    var value1 = Pop();
                    Push(Format("({0} - {1})", value1, value2), value1.type);
                }
                else
                {
                    throw new NotImplementedException(string.Format("Undefined operation code: {0}", instruction.OpCode));
                }
            }

            WriteLine("void main() {");

            foreach (var @object in stack.Reverse())
            {
                WriteLine(string.Format("{0};", @object.ToString()));
            }

            WriteLine("}");

            return writer.ToString();
        }
    }
    
    public class ScriptingMaterial : Material
    {
        internal static Dictionary<Type, Dictionary<ShaderType, string>> ShaderCache = new Dictionary<Type, Dictionary<ShaderType, string>>();

        public ScriptingMaterial()
        {
            var type = this.GetType();
            Build(type);
            shader = new GPUProgram(ShaderCache[type][ShaderType.VertexShader], ShaderCache[type][ShaderType.FragmentShader]);
        }

        internal static void Prebuild()
        {
            Debug.Log("Prebuild scripting materials");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(ScriptingMaterial).IsAssignableFrom(type))
                    {
                        continue;
                    }
                    if (type == typeof(ScriptingMaterial))
                    {
                        continue;
                    }
                    Build(type);
                }
            }
        }

        internal static void Build(Type type)
        {
            if (ShaderCache.ContainsKey(type))
            {
                return;
            }
            Debug.LogFormat("Building material {0}", type.FullName);
            if (!type.IsSubclassOf(typeof(ScriptingMaterial)))
            {
                Debug.LogErrorFormat("Can not build material {0}", type.FullName);
                return;
            }

            // vertex shader
            var vertexShaderMethodReference = type.GetTypeDefinition().FindMethod("VertexShader");
            string vertexShaderSource = new IntermediateLanguageCompiler().Compile(vertexShaderMethodReference);

            // fragment shader
            var frahmentShaderMethodReference = type.GetTypeDefinition().FindMethod("FragmentShader");
            string fragmentShaderSource = new IntermediateLanguageCompiler().Compile(frahmentShaderMethodReference);

            // add into cache
            ShaderCache.Add(type, new Dictionary<ShaderType, string>()
            {
                { ShaderType.VertexShader, vertexShaderSource },
                { ShaderType.FragmentShader, fragmentShaderSource },
            });

            Debug.LogFormat("Material {0}\nVertex Shader:\n{1}Fragment Shader:\n{2}", type.FullName, vertexShaderSource, fragmentShaderSource);
        }

        [Shader(OpenGL: "texture({0}, {1})")]
        public static Vector4 TextureSample(Texture texture, Vector2 uv)
        {
            throw new NotImplementedException();
        }
    }
}
