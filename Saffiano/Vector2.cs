using System;

namespace Saffiano
{
    [Shader(OpenGL: "vec2")]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return this.x;
                }
                else if (index == 1)
                {
                    return this.y;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                if (index == 0)
                {
                    this.x = value;
                }
                else if (index == 1)
                {
                    this.y = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Vector2 zero => new Vector2(0, 0);

        [Shader(OpenGL: "({0} + {1})")]
        public static Vector2 operator+(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        [Shader(OpenGL: "({0} - {1})")]
        public static Vector2 operator-(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector2 operator*(Vector2 a, float b)
        {
            return new Vector2(a.x * b, a.y * b);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector2 operator*(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector2 operator*(float a, Vector2 b)
        {
            return new Vector2(a * b.x, a * b.y);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector2 operator/(Vector2 a, float b)
        {
            return new Vector2(a.x / b, a.y / b);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector2 operator/(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector2 operator/(float a, Vector2 b)
        {
            return new Vector2(a / b.x, a / b.y);
        }

        [Shader(OpenGL: "(-{0})")]
        public static Vector2 operator-(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;

        public static bool operator !=(Vector2 a, Vector2 b) => a.x != b.x || a.y != b.y;

        public override String ToString()
        {
            return String.Format("({0}, {1})", this.x, this.y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
            {
                return false;
            }
            Vector2 v = (Vector2)obj;
            return x == v.x && y == v.y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public float magnitude
        {
            [Shader(OpenGL: "length({0})")]
            get => Mathf.Sqrt(x * x + y * y);
        }

        public Vector2 normalized
        {
            get
            {
                var length = magnitude;
                if (length > 0)
                {
                    return this / length;
                }
                return new Vector2(x, y);
            }
        }
    }
}
