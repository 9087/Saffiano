using System.Collections.Generic;

namespace Saffiano
{
    public enum ShaderType
    {
        VertexShader = 0,
        FragmentShader = 35632,
    }

    public abstract class Material
    {
        public GPUProgram shader { get; internal set; }

        internal HashSet<Uniform> uniforms { get; set; }

        protected Material()
        {
        }
    }
}
