using System;

namespace Saffiano
{
    [Shader(OpenGL: "vec3")]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }
        public Vector3(Vector2 v, float z) : this(v.x, v.y, z)
        {
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Vector3 left
        {
            get
            {
                return new Vector3(-1, 0, 0);
            }
        }

        public static Vector3 down
        {
            get
            {
                return new Vector3(0, -1, 0);
            }
        }

        public static Vector3 up
        {
            get
            {
                return new Vector3(0, 1, 0);
            }
        }

        public static Vector3 back
        {
            get
            {
                return new Vector3(0, 0, -1);
            }
        }

        public static Vector3 forward
        {
            get
            {
                return new Vector3(0, 0, 1);
            }
        }

        public static Vector3 one
        {
            get
            {
                return new Vector3(1, 1, 1);
            }
        }

        public static Vector3 zero
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }

        public static Vector3 right
        {
            get
            {
                return new Vector3(1, 0, 0);
            }
        }

        public Vector2 xy
        {
            get
            {
                return new Vector2(x, y);
            }
        }

        public float sqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public float magnitude
        {
            [Shader(OpenGL: "length({0})")]
            get
            {
                return Mathf.Sqrt(this.sqrMagnitude);
            }
        }

        public Vector3 normalized
        {
            [Shader(OpenGL: "normalize({0})")]
            get
            {
                float _magnitude = this.magnitude;
                if (Mathf.Approximately(_magnitude, 0))
                {
                    return Vector3.zero;
                }
                return this / this.magnitude;
            }
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            float mod = from.sqrMagnitude * to.sqrMagnitude;
            float dot = Mathf.Clamp(Vector3.Dot(from, to) / Mathf.Sqrt(mod), -1.0f, 1.0f);
            return MathF.Acos(dot) * Mathf.Rad2Deg;
        }

        [Shader(OpenGL: "cross({0}, {1})")]
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.y * rhs.z - rhs.y * lhs.z, rhs.x * lhs.z - lhs.x * rhs.z, lhs.x * rhs.y - rhs.x * lhs.y);
        }

        [Shader(OpenGL: "dot({0}, {1})")]
        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        [Shader(OpenGL: "reflect({0}, {1})")]
        public static Vector3 Reflect(Vector3 i, Vector3 n)
        {
            return i - 2.0f * Vector3.Dot(n, i) * n;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
            {
                return false;
            }

            var vector = (Vector3)obj;
            return x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override int GetHashCode()
        {
            var hashCode = 373119288;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", this.x, this.y, this.z);
        }

        [Shader(OpenGL: "({0} + {1})")]
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [Shader(OpenGL: "(-{0})")]
        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        [Shader(OpenGL: "({0} - {1})")]
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        [Shader(OpenGL: "({0} * {1})")]
        public static Vector3 operator *(float d, Vector3 a)
        {
            return new Vector3(d * a.x, d * a.y, d * a.z);
        }

        [Shader(OpenGL: "({0} / {1})")]
        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
    }
}
