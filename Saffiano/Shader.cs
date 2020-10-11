using System;

namespace Saffiano
{
    internal class ShaderAttribute : Attribute
    {
        public string pattern { get; private set; }

        public ShaderAttribute(string OpenGL)
        {
            pattern = OpenGL;
        }
    }
}
