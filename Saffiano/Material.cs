using System.Collections.Generic;

namespace Saffiano
{
    internal enum ShaderType
    {
        VertexShader = 0,
        FragmentShader = 35632,
    }

    public abstract class Material
    {
        internal GPUProgram shader { get; set; }

        internal HashSet<Uniform> uniforms { get; set; }

        protected Material()
        {
        }
    }
}
