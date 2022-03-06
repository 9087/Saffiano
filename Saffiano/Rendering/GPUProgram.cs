using System;
using System.IO;

namespace Saffiano.Rendering
{
    public class GPUProgram
    {
        private string GetShaderSourceCode(ShaderType shaderType)
        {
            if (shaderSourceData.codes.TryGetValue(shaderType, out string code))
            {
                return code;
            }
            return null;
        }

        internal string vertexShaderSourceCode => GetShaderSourceCode(ShaderType.VertexShader);

        internal TessellationConfiguration tessellationConfiguration => shaderSourceData.tessellationConfiguration;

        internal string tessEvaluationShaderSourceCode => GetShaderSourceCode(ShaderType.TessEvaluationShader);

        internal string tessControlShaderSourceCode => GetShaderSourceCode(ShaderType.TessControlShader);

        internal string geometryShaderSourceCode => GetShaderSourceCode(ShaderType.GeometryShader);

        internal string fragmentShaderSourceCode => GetShaderSourceCode(ShaderType.FragmentShader);

        internal CullMode cullMode { get; private set; }

        internal ZTest zTest { get; private set; }

        internal Blend blend { get; private set; }

        protected ShaderSourceData shaderSourceData { get; private set; }

        private static string ReadFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        public GPUProgram(
            ShaderSourceData shaderSourceData,
            CullMode cullMode, ZTest zTest, Blend blend
        )
        {
            this.shaderSourceData = shaderSourceData;
            this.cullMode = cullMode;
            this.zTest = zTest;
            this.blend = blend;
        }

        public override bool Equals(object other)
        {
            if (!(other is GPUProgram))
            {
                return false;
            }
            var otherGPUProgram = other as GPUProgram;
            return this.vertexShaderSourceCode == otherGPUProgram.vertexShaderSourceCode &&
                this.fragmentShaderSourceCode == otherGPUProgram.fragmentShaderSourceCode &&
                this.cullMode == otherGPUProgram.cullMode;
        }

        public override int GetHashCode()
        {
            return this.vertexShaderSourceCode.GetHashCode() ^ this.fragmentShaderSourceCode.GetHashCode();
        }

        public static GPUProgram LoadFromFile(string vertexShaderFilePath, string fragmentShaderFilePath)
        {
            ShaderSourceData shaderSourceData = new ShaderSourceData();
            shaderSourceData.codes[ShaderType.VertexShader] = ReadFile(vertexShaderFilePath);
            shaderSourceData.codes[ShaderType.FragmentShader] = ReadFile(fragmentShaderFilePath);
            return new GPUProgram(shaderSourceData, CullMode.Off, ZTest.Less, Blend.off);
        }
    }
}
