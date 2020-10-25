using System;

namespace Saffiano
{
    [Shader(OpenGL: "vec4")]
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b) : this(r, g, b, 1)
        {
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        [Shader(OpenGL: "{0}")]
        public static explicit operator Color(Vector4 vector4)
        {
            return new Color(vector4.x, vector4.y, vector4.z, vector4.w);
        }
    }
}
