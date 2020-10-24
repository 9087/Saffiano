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
        public static float Max(float x, float y)
        {
            return MathF.Max(x, y);
        }
    }
}
