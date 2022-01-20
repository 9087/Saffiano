using Saffiano.Rendering;
using System;

namespace Saffiano
{
    [Shader(OpenGL: "vec4")]
    public struct Color : IEquatable<Color>
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

        public static bool operator ==(Color a, Color b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        public static bool operator !=(Color a, Color b)
        {
            return !(a == b);
        }

        public static Color black => new Color(0, 0, 0, 1);

        public static Color blue => new Color(0, 0, 1, 1);

        public static Color clear => new Color(0, 0, 0, 0);

        public static Color cyan => new Color(0, 1, 1, 1);

        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1);

        public static Color green => new Color(0, 1, 0, 1);

        public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1);

        public static Color magenta => new Color(1, 0, 1, 1);

        public static Color red => new Color(1, 0, 0, 1);

        public static Color white => new Color(1, 1, 1, 1);

        public static Color yellow => new Color(1, 0.92f, 0.016f, 1);

        public override bool Equals(object obj)
        {
            return obj is Color color && Equals(color);
        }

        public bool Equals(Color other)
        {
            return r == other.r &&
                   g == other.g &&
                   b == other.b &&
                   a == other.a;
        }

        public override int GetHashCode()
        {
            int hashCode = -490236692;
            hashCode = hashCode * -1521134295 + r.GetHashCode();
            hashCode = hashCode * -1521134295 + g.GetHashCode();
            hashCode = hashCode * -1521134295 + b.GetHashCode();
            hashCode = hashCode * -1521134295 + a.GetHashCode();
            return hashCode;
        }
    }
}
