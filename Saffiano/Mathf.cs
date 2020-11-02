using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public struct Mathf
    {
        public const float PI = 3.14159274F;
        public const float Infinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float Deg2Rad = 0.0174532924F;
        public const float Rad2Deg = 57.29578F;
        public const float Epsilon = float.Epsilon;

        public static int Abs(int value)
        {
            return (int)(MathF.Abs(value));
        }

        public static float Abs(float f)
        {
            return MathF.Abs(f);
        }

        public static float Clamp(float value, float min, float max)
        {
            return ((value) > (max) ? (max) : ((value) < (min) ? (min) : value));
        }

        public static bool Approximately(float a, float b)
        {
            return Mathf.Abs(a - b) < Mathf.Epsilon;
        }

        public static float Sqrt(float f)
        {
            return MathF.Sqrt(f);
        }

        public static float Acos(float f)
        {
            return MathF.Acos(f);
        }

        public static float Sign(float f)
        {
            return MathF.Sign(f);
        }

        [Shader(OpenGL: "max({0}, {1})")]
        public static float Max(float a, float b)
        {
            return MathF.Max(a, b);
        }

        [Shader(OpenGL: "max({0}, {1})")]
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y));
        }

        [Shader(OpenGL: "max({0}, {1})")]
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y), MathF.Max(a.z, b.z));
        }

        [Shader(OpenGL: "max({0}, {1})")]
        public static Vector4 Max(Vector4 a, Vector4 b)
        {
            return new Vector4(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y), MathF.Max(a.z, b.z), MathF.Max(a.w, b.w));
        }

        [Shader(OpenGL: "min({0}, {1})")]
        public static float Min(float a, float b)
        {
            return MathF.Min(a, b);
        }

        [Shader(OpenGL: "min({0}, {1})")]
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y));
        }

        [Shader(OpenGL: "min({0}, {1})")]
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y), MathF.Min(a.z, b.z));
        }

        [Shader(OpenGL: "min({0}, {1})")]
        public static Vector4 Min(Vector4 a, Vector4 b)
        {
            return new Vector4(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y), MathF.Min(a.z, b.z), MathF.Min(a.w, b.w));
        }
    }
}
