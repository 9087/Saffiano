using System.Collections.Generic;

namespace Saffiano
{
    public enum ShaderType
    {
        VertexShader = 0,
        FragmentShader = 35632,
    }

    public enum CullMode : uint
    {
        Off = 0,
        Front = 1,
        Back = 2,
        FrontAndBack = 3,
    }

    public abstract class Material : Asset
    {
        public virtual Saffiano.Rendering.GPUProgram shader { get; }

        internal HashSet<Uniform> uniforms { get; set; }

        public virtual CullMode cullMode { get; set; } = CullMode.Back;

        protected Material()
        {
        }
    }
}
