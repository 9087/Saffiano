using System;
using System.IO;

namespace Saffiano
{
    public class GPUProgram : IEquatable<GPUProgram>
    {
        internal string vertexShaderSourceCode
        {
            get; private set;
        }

        internal string fragmentShaderSourceCode
        {
            get; private set;
        }

        private static string ReadFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        public GPUProgram(string vertexShaderSourceCode, string fragmentShaderSourceCode)
        {
            this.vertexShaderSourceCode = vertexShaderSourceCode;
            this.fragmentShaderSourceCode = fragmentShaderSourceCode;
        }

        public bool Equals(GPUProgram other)
        {
            return this.vertexShaderSourceCode == other.vertexShaderSourceCode && this.fragmentShaderSourceCode == other.fragmentShaderSourceCode;
        }

        public static GPUProgram LoadFromFile(string vertexShaderFilePath, string fragmentShaderFilePath)
        {
            return new GPUProgram(ReadFile(vertexShaderFilePath), ReadFile(fragmentShaderFilePath));
        }
    }
}
