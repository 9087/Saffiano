using System;

namespace Saffiano
{
    [Shader(OpenGL: "vec4")]
    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector2 xy => new Vector2(x, y);

        public Vector2 zw => new Vector2(z, w);

        public Vector2 xw => new Vector2(x, w);

        public Vector2 zy => new Vector2(z, y);

        public Vector3 xyz => new Vector3(x, y, z);

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector3 v, float w) : this(v.x, v.y, v.z, w)
        {
        }

        public Vector4(Vector2 a, Vector2 b) : this(a.x, a.y, b.x, b.y)
        {
        }

        public override string ToString()
        {
            return String.Format("({0:F2}, {1:F2}, {2:F2}, {3:F2})", this.x, this.y, this.z, this.w);
        }

        [Shader(OpenGL: "{0}")]
        public static explicit operator Vector4(Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        [Shader(OpenGL: "({0} + {1})")]
        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        [Shader(OpenGL: "(-{0})")]
        public static Vector4 operator -(Vector4 a)
        {
            return new Vector4(-a.x, -a.y, -a.z, -a.w);
        }

        [Shader(OpenGL: "({0} - {1})")]
        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector4 operator *(Vector4 a, float d)
        {
            return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector4 operator *(float d, Vector4 a)
        {
            return new Vector4(d * a.x, d * a.y, d * a.z, d * a.w);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector4 operator /(Vector4 a, float d)
        {
            return new Vector4(a.x / d, a.y / d, a.z / d, a.w / d);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector4 operator *(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }
    }
}
