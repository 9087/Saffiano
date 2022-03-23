using _OpenGL = OpenGL;
using OpenGL;
using System.Text;

namespace Saffiano.Rendering.OpenGL
{
    internal class ShaderData
    {
        public uint program { get; internal set; }

        public uint vs { get; internal set; }

        public uint tcs { get; internal set; } = 0;

        public uint tes { get; internal set; } = 0;

        public uint gs { get; internal set; } = 0;

        public uint fs { get; internal set; }
    }

    internal class ShaderCache : Cache<ShaderSourceData, ShaderData>
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

        protected override ShaderData OnRegister(ShaderSourceData key)
        {
            ShaderData shaderData = new ShaderData();
            shaderData.program = Gl.CreateProgram();
            shaderData.vs = Compile(_OpenGL.ShaderType.VertexShader, key.vertexShaderSourceCode);
            shaderData.fs = Compile(_OpenGL.ShaderType.FragmentShader, key.fragmentShaderSourceCode);
            Gl.AttachShader(shaderData.program, shaderData.vs);
            Gl.AttachShader(shaderData.program, shaderData.fs);
            if (key.tessControlShaderSourceCode != null && Gl.CurrentVersion.Major >= 4)
            {
                shaderData.tcs = Compile(_OpenGL.ShaderType.TessControlShader, key.tessControlShaderSourceCode);
                Gl.AttachShader(shaderData.program, shaderData.tcs);
            }
            if (key.tessEvaluationShaderSourceCode != null && Gl.CurrentVersion.Major >= 4)
            {
                shaderData.tes = Compile(_OpenGL.ShaderType.TessEvaluationShader, key.tessEvaluationShaderSourceCode);
                Gl.AttachShader(shaderData.program, shaderData.tes);
            }
            if (key.geometryShaderSourceCode != null)
            {
                shaderData.gs = Compile(_OpenGL.ShaderType.GeometryShader, key.geometryShaderSourceCode);
                Gl.AttachShader(shaderData.program, shaderData.gs);
            }
            Gl.LinkProgram(shaderData.program);
            return shaderData;
        }

        protected override void OnUnregister(ShaderSourceData key)
        {
            var shaderData = this[key];
            Gl.DeleteShader(shaderData.vs);
            Gl.DeleteShader(shaderData.fs);
            if (shaderData.tcs != 0)
            {
                Gl.DeleteShader(shaderData.tcs);
            }
            if (shaderData.tes != 0)
            {
                Gl.DeleteShader(shaderData.tes);
            }
            if (shaderData.gs != 0)
            {
                Gl.DeleteShader(shaderData.gs);
            }
            Gl.DeleteProgram(shaderData.program);
        }
    }
}
