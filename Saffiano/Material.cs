using Mono.Cecil;
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

    public class Uniform : IEquatable<Uniform>
    {
        public PropertyInfo propertyInfo { get; private set; }

        public string name => propertyInfo.Name;

        public Type type => propertyInfo.PropertyType;

        public Uniform(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public bool Equals(Uniform other)
        {
            return this.propertyInfo == other.propertyInfo;
        }
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
        internal GPUProgram shader { get; set; }

        protected Material()
        {
        }

        public Material(string vertexShaderFilePath, string fragmentShaderFilePath) : this()
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

        public static MethodBase GetMethodInfo(this MethodReference methodReference)
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
            if (methodDefinition.IsConstructor)
            {
                return declaringType.GetConstructor(all, null, parameterTypes, null);
            }
            return declaringType.GetMethod(methodReference.Name, all, null, parameterTypes, null);
        }

        public static PropertyDefinition FindPropertyDefinitionIncludeAncestors(this TypeDefinition typeDefinition, string name)
        {
            while (typeDefinition != null)
            {
                foreach (var property in typeDefinition.Properties)
                {
                    if (property.Name == name)
                    {
                        return property;
                    }
                }
                typeDefinition = typeDefinition.BaseType.Resolve();
            }
            return null;
        }
    }

    internal class IntermediateLanguageCompiler
    {
        private TextWriter writer = new StringWriter();
        private HashSet<Uniform> uniforms = null;

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

        private LocalVariable GetLocalVariable(uint index, TypeReference type = null)
        {
            if (!localVariables.ContainsKey(index))
            {
                if (type != null)
                {
                    localVariables[index] = new LocalVariable(string.Format("{0}_{1}", type.FullName.Replace('.', '_'), index), type);
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                if (localVariables[index].type != type && type != null)
                {
                    throw new Exception();
                }
            }
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

        private Element Peek()
        {
            return stack.Peek();
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
            var methodInfo = methodReference.GetMethodInfo();
            var shaderAttributes = methodInfo.GetCustomAttributes<ShaderAttribute>();
            if (shaderAttributes.Any())
            {
                return shaderAttributes.First().pattern;
            }
            var md = methodReference.Resolve();
            if (md.IsConstructor)
            {
                var parameterPatterns = new List<string>();
                for (int i = 0; i < md.Parameters.Count; i++)
                {
                    parameterPatterns.Add(string.Format("{{{0}}}", i));
                }
                return string.Format("{0}({1})", OnCompilingType(md.DeclaringType), string.Join(", ", parameterPatterns));
            }
            Debug.LogErrorFormat("Method \"{0}.{1}\" is not a built-in shader method.", methodInfo.DeclaringType.FullName, methodInfo.Name);
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
                else if (@object is LocalVariable)
                {
                    list.Add((@object as LocalVariable).name);
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
                var element = Pop();
                var typeDefinition = element.type as TypeDefinition;
                var propertyDefinition = typeDefinition.FindPropertyDefinitionIncludeAncestors(propertyName);

                if (propertyDefinition != null && methodReference.Parameters.Count() == 0 && methodReference.HasThis)
                {
                    // call property getter
                    if (typeof(ScriptingMaterial).IsAssignableFrom(typeDefinition.GetRuntimeType()))
                    {
                        // property defined in ScriptingMaterial(uniform type)
                        Push(propertyDefinition.Name, propertyDefinition.PropertyType);
                        var propertyInfo = propertyDefinition.DeclaringType.GetRuntimeType().GetProperty(propertyDefinition.Name);
                        if (propertyInfo.GetCustomAttribute<UniformAttribute>() != null)
                        {
                            this.uniforms.Add(new Uniform(propertyInfo));
                        }
                        else
                        {
                            throw new Exception("uncertain material property get behaviour");
                        }
                    }
                    else
                    {
                        // other property
                        var methodInfo = methodReference.GetMethodInfo();
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
                        Push(Format(pattern, element, propertyDefinition.Name), propertyDefinition.PropertyType);
                    }
                    return true;
                }
            }
            var s = Format(OnCompilingMethod(methodReference), Pop(methodReference.Parameters.Count()).ToArray());
            if (methodReference.Resolve().IsConstructor)
            {
                if (Peek().@object is LocalVariable)
                {
                    var element = Pop();
                    Push(Format("{0} {1} = {2}", methodReference.DeclaringType, element.@object, s), methodReference.ReturnType);
                    return true;
                }
            }
            Push(s, methodReference.ReturnType);
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

        public string Compile(MethodReference methodReference, out List<Uniform> uniformList)
        {
            if (methodReference == null)
            {
                uniformList = null;
                return null;
            }

            // used uniforms
            this.uniforms = new HashSet<Uniform>();

            writer.Flush();

            var methodDefinition = methodReference.Resolve();
            var typeDefinition = methodDefinition.DeclaringType;
            var type = typeDefinition.GetRuntimeType();
            var methodInfo = methodDefinition.GetMethodInfo();

            WriteLine("#version 330 core");

            // Method body

            // Reference: ECMA-335
            // http://www.ecma-international.org/publications/standards/Ecma-335.htm

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
                else if (
                    instruction.OpCode == OpCodes.Stloc_0 ||
                    instruction.OpCode == OpCodes.Stloc_1 ||
                    instruction.OpCode == OpCodes.Stloc_2 ||
                    instruction.OpCode == OpCodes.Stloc_3 ||
                    instruction.OpCode == OpCodes.Stloc_S
                )
                {
                    // stloc.(0~3) - Pop a value from stack into local variable (0~3).
                    int stlocIndex;
                    if (instruction.OpCode == OpCodes.Stloc_S)
                    {
                        stlocIndex = (instruction.Operand as VariableDefinition).Index;
                    }
                    else
                    {
                        stlocIndex = new Dictionary<OpCode, int> {
                            { OpCodes.Stloc_0, 0 },
                            { OpCodes.Stloc_1, 1 },
                            { OpCodes.Stloc_2, 2 },
                            { OpCodes.Stloc_3, 3 }
                        }[instruction.OpCode];
                    }
                    var element = Pop();
                    var localVariable = GetLocalVariable((uint)stlocIndex, element.type);
                    Push(Format("{0} {1} = {2}", localVariable.type, localVariable.name, element.@object));
                }
                else if (
                    instruction.OpCode == OpCodes.Ldloc_0 ||
                    instruction.OpCode == OpCodes.Ldloc_1 ||
                    instruction.OpCode == OpCodes.Ldloc_2 ||
                    instruction.OpCode == OpCodes.Ldloc_3
                )
                {
                    // ldloc.(0~3) - Load local variable (0~3) onto stack.
                    int ldlocIndex = new Dictionary<OpCode, int> {
                        { OpCodes.Ldloc_0, 0 },
                        { OpCodes.Ldloc_1, 1 },
                        { OpCodes.Ldloc_2, 2 },
                        { OpCodes.Ldloc_3, 3 }
                    }[instruction.OpCode];
                    var localVariable = GetLocalVariable((uint)ldlocIndex);
                    Push(localVariable.name, localVariable.type);
                }
                else if (instruction.OpCode == OpCodes.Ldloca_S)
                {
                    // ldloca.s indx - Load address of local variable with index indx, short form.
                    var vd = instruction.Operand as VariableDefinition;
                    var localVariable = GetLocalVariable((uint)vd.Index, vd.VariableType);
                    Push(localVariable, vd.VariableType);
                }
                else if (instruction.OpCode == OpCodes.Ldfld)
                {
                    // ldfld field - Push the value of field of object (or value type) obj, onto the stack.
                    var fd = instruction.Operand as FieldDefinition;
                    Push(Format("({0}).{1}", Pop(), fd.Name), fd.FieldType);
                }
                else if (instruction.OpCode == OpCodes.Add)
                {
                    // add - Add two values, returning a new value.
                    var value2 = Pop();
                    var value1 = Pop();
                    Push(Format("({0} + {1})", value1, value2), value1.type);
                }
                else if (instruction.OpCode == OpCodes.Sub)
                {
                    // sub - Subtract value2 from value1, returning a new value.
                    var value2 = Pop();
                    var value1 = Pop();
                    Push(Format("({0} - {1})", value1, value2), value1.type);
                }
                else if (instruction.OpCode == OpCodes.Neg)
                {
                    // Neg - Negate value. 
                    var value = Pop();
                    Push(Format("(-{0})", value), value.type);
                }
                else
                {
                    throw new NotImplementedException(string.Format("Undefined operation code: {0}", instruction.OpCode));
                }
            }

            var properties = type.GetProperties();
            foreach (var uniform in this.uniforms)
            {
                WriteLine(Format("uniform {0} {1};", uniform.propertyInfo.PropertyType, uniform.propertyInfo.Name));
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

            WriteLine("void main() {");

            foreach (var @object in stack.Reverse())
            {
                WriteLine(string.Format("{0};", @object.ToString()));
            }

            WriteLine("}");

            uniformList = this.uniforms.ToList();
            this.uniforms = null;

            return writer.ToString();
        }
    }
    
    internal class ShaderSourceData
    {
        public Dictionary<ShaderType, string> codes { get; private set; }

        public HashSet<Uniform> uniforms { get; private set; }

        public ShaderSourceData()
        {
            codes = new Dictionary<ShaderType, string>();
            uniforms = new HashSet<Uniform>();
        }
    }

    public class ScriptingMaterial : Material
    {
        internal static Dictionary<Type, ShaderSourceData> ShaderSourceCache = new Dictionary<Type, ShaderSourceData>();

        [Uniform]
        public Matrix4x4 mvp { get; set; }

        [Uniform]
        public Matrix4x4 mv { get; set; }

        [Uniform]
        public Texture texture { get; set; }

        public ScriptingMaterial()
        {
            var type = this.GetType();
            Build(type);
            shader = new GPUProgram(ShaderSourceCache[type].codes[ShaderType.VertexShader], ShaderSourceCache[type].codes[ShaderType.FragmentShader]);
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
            if (ShaderSourceCache.ContainsKey(type))
            {
                return;
            }
            Debug.LogFormat("Building material {0}", type.FullName);
            if (!type.IsSubclassOf(typeof(ScriptingMaterial)))
            {
                Debug.LogErrorFormat("Can not build material {0}", type.FullName);
                return;
            }

            ShaderSourceData shaderSourceData = new ShaderSourceData();
            foreach (ShaderType shaderType in typeof(ShaderType).GetEnumValues())
            {
                var methodReference = type.GetTypeDefinition().FindMethod(shaderType.ToString());
                string source = new IntermediateLanguageCompiler().Compile(methodReference, out var uniformList);
                var description = string.Format("// {0} generated from {1}\n", shaderType.ToString(), type.FullName);
                shaderSourceData.codes[shaderType] = description + source;
                foreach (var uniform in uniformList)
                {
                    shaderSourceData.uniforms.Add(uniform);
                }
            }
            ShaderSourceCache.Add(type, shaderSourceData);
        }

        [Shader(OpenGL: "texture({0}, {1})")]
        public static Vector4 TextureSample(Texture texture, Vector2 uv)
        {
            throw new NotImplementedException();
        }
    }
}
