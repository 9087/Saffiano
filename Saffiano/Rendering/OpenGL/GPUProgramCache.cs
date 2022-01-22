using _OpenGL = OpenGL;
using OpenGL;
using System.Text;

namespace Saffiano.Rendering.OpenGL
{
    internal class GPUProgramData
    {
        public uint program { get; internal set; }

        public uint vs { get; internal set; }

        public uint fs { get; internal set; }
    }

    internal class GPUProgramCache : Cache<GPUProgram, GPUProgramData>
    {
        private uint Compile(_OpenGL.ShaderType shaderType, string source)
        {
            uint shader = Gl.CreateShader(shaderType);

            Gl.ShaderSource(shader, new string[] { source });
            Gl.CompileShader(shader);
            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
            if (success == Gl.FALSE)
            {
                // compile error
                Gl.GetShader(shader, ShaderParameterName.InfoLogLength, out int length);
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.EnsureCapacity(length);
                Gl.GetShaderInfoLog(shader, length, out int _, logBuilder);
                Debug.LogWarning(string.Format("Shader compilation failed:\n{0}\n{1}", source, logBuilder.ToString()));
            }
            return shader;
        }

        protected override GPUProgramData OnRegister(GPUProgram key)
        {
            GPUProgramData shaderData = new GPUProgramData();
            shaderData.program = Gl.CreateProgram();
            shaderData.vs = Compile(_OpenGL.ShaderType.VertexShader, key.vertexShaderSourceCode);
            shaderData.fs = Compile(_OpenGL.ShaderType.FragmentShader, key.fragmentShaderSourceCode);
            Gl.AttachShader(shaderData.program, shaderData.vs);
            Gl.AttachShader(shaderData.program, shaderData.fs);
            Gl.LinkProgram(shaderData.program);
            return shaderData;
        }

        protected override void OnUnregister(GPUProgram key)
        {
            Gl.DeleteShader(this[key].vs);
            Gl.DeleteShader(this[key].fs);
            Gl.DeleteProgram(this[key].program);
        }
    }
}
