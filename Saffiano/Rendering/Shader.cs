using System;

namespace Saffiano.Rendering
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
