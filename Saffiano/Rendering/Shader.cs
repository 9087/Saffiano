using System;

namespace Saffiano.Rendering
{
    public enum ShaderExtension
    {
        Quaternion,
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class ShaderAttribute : Attribute
    {
        public string pattern { get; private set; }

        public ShaderExtension[] extensions { get; private set; }

        public ShaderAttribute(string OpenGL, ShaderExtension[] extensions = null)
        {
            this.pattern = OpenGL;
            this.extensions = extensions;
        }
    }
}
