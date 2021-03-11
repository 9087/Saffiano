using Saffiano.ShaderCompilation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Saffiano
{
    public enum AttributeType : uint
    {
        Position = 0,
        Normal = 1,
        TexCoord = 2,
        Color = 3,
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AttributeAttribute : Attribute
    {
        public AttributeType type { get; private set; }

        public AttributeAttribute(AttributeType type)
        {
            this.type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniformAttribute : Attribute
    {
    }

    public class Uniform : IPrimitive
    {
        public PropertyInfo propertyInfo { get; private set; }

        public string name => propertyInfo.Name;

        public Type type => propertyInfo.PropertyType;

        public Uniform(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Uniform))
            {
                return false;
            }
            return this.propertyInfo == (obj as Uniform).propertyInfo;
        }

        public override int GetHashCode()
        {
            return this.propertyInfo.GetHashCode();
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

    public class ScriptableMaterial : Material
    {
        private static Dictionary<Type, ShaderSourceData> ShaderSourceCache = new Dictionary<Type, ShaderSourceData>();

        internal static ShaderSourceData GetShaderSourceData(Type type)
        {
            if (ShaderSourceCache.TryGetValue(type, out var data))
            {
                return data;
            }
            return null;
        }

        internal static void SetShaderSourceData(Type type, ShaderSourceData shaderSourceData)
        {
            ShaderSourceCache[type] = shaderSourceData;
        }

        [Uniform]
        public Matrix4x4 mvp { get; }

        [Uniform]
        public Matrix4x4 mv { get; }

        [Uniform]
        public Texture mainTexture { get; }

        public ScriptableMaterial()
        {
            var type = this.GetType();
            Build(type);
            var shaderSourceData = GetShaderSourceData(type);
            shader = new GPUProgram(shaderSourceData.codes[ShaderType.VertexShader], shaderSourceData.codes[ShaderType.FragmentShader]);
            uniforms = shaderSourceData.uniforms;
        }

        internal static void Prebuild()
        {
            Debug.Log("Prebuild scripting materials");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(ScriptableMaterial).IsAssignableFrom(type))
                    {
                        continue;
                    }
                    if (type == typeof(ScriptableMaterial))
                    {
                        continue;
                    }
                    Build(type);
                }
            }
        }

        internal static void Build(Type type)
        {
            if (GetShaderSourceData(type) != null)
            {
                return;
            }
            Debug.LogFormat("Building material {0}", type.FullName);
            if (!type.IsSubclassOf(typeof(ScriptableMaterial)))
            {
                Debug.LogErrorFormat("Can not build material {0}", type.FullName);
                return;
            }

            ShaderSourceData shaderSourceData = new ShaderSourceData();
            foreach (ShaderType shaderType in typeof(ShaderType).GetEnumValues())
            {
                var methodReference = type.GetTypeDefinition().FindMethod(shaderType.ToString());
                string source = new ShaderCompiler().Compile(methodReference, out var uniformList);
                var description = string.Format("// {0} generated from {1}\n", shaderType.ToString(), type.FullName);
                shaderSourceData.codes[shaderType] = description + source;
                foreach (var uniform in uniformList)
                {
                    shaderSourceData.uniforms.Add(uniform);
                }
            }
            SetShaderSourceData(type, shaderSourceData);
        }

        [Shader(OpenGL: "vec2(dFdx({0}), dFdy({0}))")]
        public static Vector2 Derivative(float value)
        {
            throw new NotImplementedException();
        }
    }
}
