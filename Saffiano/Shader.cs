using System;

namespace Saffiano
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class ShaderAttribute : Attribute
    {
        public string pattern { get; private set; }

        public ShaderAttribute(string OpenGL)
        {
            pattern = OpenGL;
        }
    }
}
