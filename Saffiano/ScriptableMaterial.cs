using Mono.Cecil;
using Saffiano.Rendering;
using Saffiano.ShaderCompilation;
using System;
using System.Collections.Generic;
using System.Linq;
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


    public enum TessellationDrawType
    {
        Vertics,
        Element,
    }

    public class TessellationConfiguration
    {
        public int patchVerticesCount { get; set; }

        public TessellationDrawType drawType { get; set; }
    }

    public class ShaderSourceData
    {
        public Dictionary<ShaderType, string> codes { get; private set; }

        public HashSet<Uniform> uniforms { get; private set; }

        public TessellationConfiguration tessellationConfiguration { get; internal set; }

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
            throw new Exception("Meaningless & Non-Runnable");
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

    public enum TessellationMode
    {
        Isolines,
        Triangles,
        Quads,
    }

    public class TessellationAttribute : Attribute
    {
        public TessellationMode mode { get; private set; }

        public bool equalSpacing { get; private set; }

        public bool pointMode { get; private set; }

        public uint[] outer { get; private set; }

        public uint[] inner { get; private set; }

        public TessellationAttribute(TessellationMode mode, uint[] outer, uint[] inner, bool equalSpacing = false, bool pointMode = false)
        {
            this.mode = mode;
            this.outer = outer;
            Debug.Assert(outer.Length <= 4);
            this.inner = inner;
            Debug.Assert(inner.Length <= 2);
            this.equalSpacing = equalSpacing;
            this.pointMode = pointMode;
        }
    }

    public class ScriptableMaterial : Material
    {
        private static Dictionary<Type, ShaderSourceData> ShaderSourceCache = new Dictionary<Type, ShaderSourceData>();

        public override GPUProgram shader
        {
            get
            {
                var shaderSourceData = GetShaderSourceData(this.GetType());
                return new GPUProgram(shaderSourceData, this.cullMode, this.zTest, this.blend);
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

        private static string GenerateShaderSourceCode(Type type, ShaderType shaderType, string methodName, out HashSet<Uniform> uniformList, Func<ParameterInfo, bool> filter = null)
        {
            var methodReference = type.GetTypeDefinition().FindMethod(methodName);
            if (methodReference == null)
            {
                uniformList = null;
                return null;
            }
            if (methodReference.DeclaringType.Resolve().GetRuntimeType() == typeof(ScriptableMaterial))
            {
                if (shaderType == ShaderType.GeometryShader ||
                    shaderType == ShaderType.TessControlShader ||
                    shaderType == ShaderType.TessEvaluationShader)
                {
                    uniformList = null;
                    return null;
                }
                throw new Exception(string.Format("override the necessary shader {0}!", methodName));
            }
            return new ShaderCompiler().Compile(methodReference, out uniformList, filter: filter);
        }

        private static void ProcessTessellationShader(Type type, ref ShaderSourceData shaderSourceData)
        {
            var methodName = CompileContext.TessellationShaderMethodName;
            var source = GenerateShaderSourceCode(type, ShaderType.TessEvaluationShader, methodName, out var uniformList, (ParameterInfo info) => info.Name != "gl_TessCoord");
            if (source == null)
            {
                return;
            }

            var methodReference = type.GetTypeDefinition().FindMethod(methodName);
            var methodDefinition = methodReference.Resolve();
            var methodInfo = methodDefinition.DeclaringType.Resolve().GetRuntimeType().GetMethod(methodDefinition.Name);

            var tessellationAttribute = methodInfo.GetCustomAttribute<TessellationAttribute>();
            Debug.Assert(tessellationAttribute != null, "TesselationShader method shall be decorated with TessellationAttribute");

            List<string> layoutParameters = new List<string>();
            layoutParameters.Add(tessellationAttribute.mode.ToString().ToLower());
            if (tessellationAttribute.equalSpacing)
            {
                layoutParameters.Add("equal_spacing");
            }
            if (tessellationAttribute.pointMode)
            {
                layoutParameters.Add("point_mode");
            }

            var description = string.Format(
                "// {0} generated from {1}\n" +
                "#version 400\n" +
                "layout({2}) in;\n",
                methodName, type.FullName, string.Join(", ", layoutParameters)
            ); ;
            shaderSourceData.codes[ShaderType.TessEvaluationShader] = description + source;

            if (uniformList != null)
            {
                foreach (var uniform in uniformList)
                {
                    shaderSourceData.uniforms.Add(uniform);
                }
            }

            var tessellationConfiguration = new TessellationConfiguration();
            switch (tessellationAttribute.mode)
            {
                case TessellationMode.Isolines:
                    tessellationConfiguration.patchVerticesCount = 2;
                    break;
                case TessellationMode.Triangles:
                    tessellationConfiguration.patchVerticesCount = 3;
                    break;
                case TessellationMode.Quads:
                    tessellationConfiguration.patchVerticesCount = 4;
                    break;
                default:
                    throw new NotImplementedException();
            }
            tessellationConfiguration.drawType = TessellationDrawType.Vertics;
            shaderSourceData.tessellationConfiguration = tessellationConfiguration;

            shaderSourceData.codes[ShaderType.TessControlShader] =

                string.Format(
                    "// Tessellation control shader generated from {0}\n",
                    type.FullName
                ) +

                "#version 400\n" +

                string.Format(
                    "layout(vertices={0}) out;\n",
                    tessellationConfiguration.patchVerticesCount
                ) +

                "void main()\n" +
                "{\n" +
                "    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;\n" +
                "    if (gl_InvocationID == 0) {\n" +

                string.Join(
                    "",
                    tessellationAttribute.outer.Select((x, i) => string.Format("gl_TessLevelOuter[{0}] = float({1});\n", i, x))
                ) +

                string.Join(
                    "",
                    tessellationAttribute.inner.Select((x, i) => string.Format("gl_TessLevelInner[{0}] = float({1});\n", i, x))
                ) +

                "    }\n" +
                "}\n";
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
                if (shaderType == ShaderType.TessControlShader)
                {
                    // Ignore tessellstion control shader
                    continue;
                }
                if (shaderType == ShaderType.TessEvaluationShader)
                {
                    ProcessTessellationShader(type, ref shaderSourceData);
                    continue;
                }

                var methodName = shaderType.ToString();
                var source = GenerateShaderSourceCode(type, shaderType, methodName, out var uniformList);
                if (source == null)
                {
                    continue;
                }
                var description = string.Format(
                    "// {0} generated from {1}\n" +
                    "#version 330 core\n",
                    methodName, type.FullName
                );
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
