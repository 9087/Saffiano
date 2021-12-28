using System;
using System.IO;

namespace Saffiano
{
    public class GPUProgram
    {
        internal string vertexShaderSourceCode { get; private set; }

        internal string fragmentShaderSourceCode { get; private set; }

        internal CullMode cullMode { get; private set; }

        private static string ReadFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        public GPUProgram(string vertexShaderSourceCode, string fragmentShaderSourceCode, CullMode cullMode)
        {
            this.vertexShaderSourceCode = vertexShaderSourceCode;
            this.fragmentShaderSourceCode = fragmentShaderSourceCode;
            this.cullMode = cullMode;
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
            return new GPUProgram(ReadFile(vertexShaderFilePath), ReadFile(fragmentShaderFilePath), CullMode.Off);
        }
    }
}
