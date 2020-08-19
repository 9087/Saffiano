using System.IO;

namespace Saffiano
{
    public class GPUProgram
    {
        internal string vertexShaderSourceCode
        {
            get; private set;
        }

        internal string fragmentShaderSourceCode
        {
            get; private set;
        }

        private string Load(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }

        public GPUProgram()
        {
        }

        public GPUProgram(string vertexShaderFilePath, string fragmentShaderFilePath)
        {
            vertexShaderSourceCode = Load(vertexShaderFilePath);
            fragmentShaderSourceCode = Load(fragmentShaderFilePath);
        }
    }

    public class DefaultGPUProgram : GPUProgram
    {
        public void Process(int a)
        {
        }
    }
}
