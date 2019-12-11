using System;

namespace Saffiano
{
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
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
                    case 3:
                        return this.w;
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
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Quaternion identity
        {
            get
            {
                return new Quaternion(0, 0, 0, 1);
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                return Quaternion.ToEulerAngles(this);
            }

            set
            {
                Quaternion q = Quaternion.Euler(x, y, z);
                this.x = q.x;
                this.y = q.y;
                this.z = q.z;
                this.w = q.w;
            }
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            Vector3 v = axis.normalized;
            float cosv = MathF.Cos(Mathf.Deg2Rad * angle * 0.5f);
            float sinv = MathF.Sin(Mathf.Deg2Rad * angle * 0.5f);
            return new Quaternion(v.x * sinv, v.y * sinv, v.z * sinv, cosv);
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            Quaternion qx = Quaternion.AngleAxis(x, new Vector3(1, 0, 0));
            Quaternion qy = Quaternion.AngleAxis(y, new Vector3(0, 1, 0));
            Quaternion qz = Quaternion.AngleAxis(z, new Vector3(0, 0, 1));
            return qx * qy * qz;
        }

        public static Quaternion Euler(Vector3 eulerAngles)
        {
            return Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            fromDirection = fromDirection.normalized;
            toDirection = fromDirection.normalized;
            if (fromDirection == toDirection)
            {
                return Quaternion.identity;
            }
            if (Mathf.Approximately(fromDirection.sqrMagnitude, 0) || Mathf.Approximately(toDirection.sqrMagnitude, 0))
            {
                return Quaternion.identity;
            }
            float angle = Vector3.Angle(fromDirection, toDirection);
            Vector3 axis = Vector3.Cross(fromDirection, toDirection);
            if (Mathf.Approximately(axis.sqrMagnitude, 0))
            {
                if (!Mathf.Approximately(fromDirection.x, 0))
                {
                    float x = -fromDirection.y / fromDirection.x;
                    float y = 1;
                    float z = 0;
                    axis = new Vector3(x, y, z);
                }
                else if (!Mathf.Approximately(fromDirection.y, 0))
                {
                    float y = -fromDirection.z / fromDirection.y;
                    float x = 0;
                    float z = 1;
                    axis = new Vector3(x, y, z);
                }
                else
                {
                    float z = -fromDirection.x / fromDirection.z;
                    float y = 0;
                    float x = 1;
                    axis = new Vector3(x, y, z);
                }
            }
            return Quaternion.AngleAxis(angle, axis);
        }

        public static Quaternion Inverse(Quaternion rotation)
        {
            return new Quaternion(-rotation.x, -rotation.y, -rotation.z, rotation.w);
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
        {
            forward = forward.normalized;
            Vector3 left = Vector3.Cross(upwards, forward).normalized;
            Vector3 third = Vector3.Cross(forward, left);

            float m00 = left.x;
            float m01 = left.y;
            float m02 = left.z;

            float m10 = third.x;
            float m11 = third.y;
            float m12 = third.z;

            float m20 = forward.x;
            float m21 = forward.y;
            float m22 = forward.z;

            float num8 = m00 + m11 + m22;

            if (num8 > 0)
            {
                float num = Mathf.Sqrt(1 + num8);
                return new Quaternion((m12 - m21) * 0.5f / num, (m20 - m02) * 0.5f / num, (m01 - m10) * 0.5f / num, num / 2);
            }
            else if (m00 >= m11 && m00 >= m22)
            {
                float num7 = Mathf.Sqrt(1 + m00 - m11 - m22);
                return new Quaternion((m01 + m10) * 0.5f / num7, (m02 + m20) * 0.5f / num7, (m12 - m21) * 0.5f / num7, num7 / 2);
            }
            else if (m11 > m22)
            {
                float num6 = Mathf.Sqrt(1 + m11 - m00 - m22);
                return new Quaternion((m10 + m01) * 0.5f / num6, 0.5f * num6, (m21 + m12) * 0.5f / num6, (m20 - m02) * 0.5f / num6);
            }
            else
            {
                float num5 = Mathf.Sqrt(1 + m22 - m00 - m11);
                return new Quaternion((m20 + m02) * 0.5f / num5, (m21 + m12) * 0.5f / num5, 0.5f * num5, (m01 - m10) * 0.5f / num5);
            }
        }

        public static Vector3 ToEulerAngles(Quaternion rotation)
        {
            float x = rotation.x;
            float y = rotation.y;
            float z = rotation.z;
            float w = rotation.w;

            float roll = MathF.Atan2(2 * (w * z + x * y), 1 - 2 * (z * z + x * x));
            float pitch = MathF.Asin(Mathf.Clamp(2 * (w * x - y * z), -1.0f, 1.0f));
            float yaw = MathF.Atan2(2 * (w * y + z * x), 1 - 2 * (x * x + y * y));

            return new Vector3(pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg, roll * Mathf.Rad2Deg);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Quaternion))
            {
                return false;
            }

            var quaternion = (Quaternion)obj;
            return x == quaternion.x &&
                    y == quaternion.y &&
                    z == quaternion.z &&
                    w == quaternion.w;
        }

        public override int GetHashCode()
        {
            var hashCode = -1743314642;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            hashCode = hashCode * -1521134295 + w.GetHashCode();
            return hashCode;
        }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            Quaternion q = rotation * new Quaternion(point.x, point.y, point.z, 0) * Quaternion.Inverse(rotation);
            return new Vector3(q.x, q.y, q.z);
        }

        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
        }

        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return !(lhs == rhs);
        }
    }

}
