using Mono.Cecil;
using Saffiano.Rendering;
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

    public class ShaderSourceData
    {
        public Dictionary<ShaderType, string> codes { get; private set; }

        public HashSet<Uniform> uniforms { get; private set; }

        public ShaderSourceData()
        {
            codes = new Dictionary<ShaderType, string>();
            uniforms = new HashSet<Uniform>();
        }
    }

    #region Primitive input/output object

    public enum InputMode
    {
        Points,
        Lines,
        Triangles,
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InputModeAttribute : Attribute
    {
        public InputMode mode { get; private set; }

        public InputModeAttribute(InputMode mode)
        {
            this.mode = mode;
        }
    }
    public enum OutputMode
    {
        Points,
        LineStrip,
        TriangleStrip,
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OutputModeAttribute : Attribute
    {
        public OutputMode mode { get; private set; }

        public int capacity { get; private set; }

        public OutputModeAttribute(OutputMode mode, int capacity)
        {
            this.mode = mode;
            this.capacity = capacity;
        }
    }

    public class Vertex
    {
        public Vector4 gl_Position { get; set; }

        public Vertex(Vector4 gl_Position)
        {
            this.gl_Position = gl_Position;
        }
    }

    internal class VertexValue : Value
    {
        public Variable gl_Position { get; set; }

        public VertexValue(TypeReference type, object name, Variable gl_Position) : base(type, name)
        {
            this.gl_Position = gl_Position;
        }
    }

    public class Input<V> where V : Vertex
    {
        public V[] vertices { get; internal set; }

        public V this[int index] => vertices[index];
    }

    public class Output<V> where V : Vertex
    {
        public void AddPrimitive(params V[] vertices)
        {
        }
    }

    #endregion

    public class ScriptableMaterial : Material
    {
        private static Dictionary<Type, ShaderSourceData> ShaderSourceCache = new Dictionary<Type, ShaderSourceData>();

        public override GPUProgram shader
        {
            get
            {
                var shaderSourceData = GetShaderSourceData(this.GetType());
                return new GPUProgram(
                    shaderSourceData.codes[ShaderType.VertexShader],
                    shaderSourceData.codes.GetValueOrDefault(ShaderType.GeometryShader, null),
                    shaderSourceData.codes[ShaderType.FragmentShader],
                    this.cullMode,
                    this.zTest,
                    this.blend
                );
            }
        }

        public static ShaderSourceData GetShaderSourceData(Type type)
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

        public ScriptableMaterial()
        {
            var type = this.GetType();
            Build(type);
            var shaderSourceData = GetShaderSourceData(type);
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
                if (methodReference == null)
                {
                    continue;
                }
                if (methodReference.DeclaringType.Resolve().GetRuntimeType() == typeof(ScriptableMaterial))
                {
                    if (shaderType == ShaderType.GeometryShader)
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception(string.Format("override the necessary shader {0}!", shaderType.ToString()));
                    }
                }
                string source = new ShaderCompiler().Compile(methodReference, out var uniformList);
                var description = string.Format("// {0} generated from {1}\n", shaderType.ToString(), type.FullName);
                shaderSourceData.codes[shaderType] = description + source;
                if (uniformList != null)
                {
                    foreach (var uniform in uniformList)
                    {
                        shaderSourceData.uniforms.Add(uniform);
                    }
                }
            }
            SetShaderSourceData(type, shaderSourceData);
        }

        protected virtual void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.Normal)] Vector3 a_normal,
            [Attribute(AttributeType.TexCoord)] Vector2 a_texcoord,
            [Attribute(AttributeType.Color)] Color a_color,
            out Vector4 gl_Position
        )
        {
            gl_Position = new Vector4(0, 0, 0, 0);
            return;  // to be overrided
        }

        protected virtual void GeometryShader(
            [InputMode(InputMode.Triangles)] Input<Vertex> input,
            [OutputMode(OutputMode.LineStrip, 16)] Output<Vertex> output
        )
        {
            return;  // to be overrided
        }

        protected virtual void FragmentShader(
            out Color f_color
        )
        {
            f_color = new Color(1, 1, 1, 1);
            return;  // to be overrided
        }

        #region Builtin uniforms

        [Uniform]
        public Matrix4x4 mvp { get; }

        [Uniform]
        public Matrix4x4 mv { get; }

        [Uniform]
        public Texture mainTexture { get; }

        #endregion
        protected Vector3 GetWorldNormal(Vector3 localNormal)
        {
            return (mv * new Vector4(localNormal, 0)).xyz.normalized;
        }
    }
}
