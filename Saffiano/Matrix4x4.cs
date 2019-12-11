using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Saffiano
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Matrix4x4
    {
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.m00;
                    case 1:
                        return this.m01;
                    case 2:
                        return this.m02;
                    case 3:
                        return this.m03;
                    case 4:
                        return this.m10;
                    case 5:
                        return this.m11;
                    case 6:
                        return this.m12;
                    case 7:
                        return this.m13;
                    case 8:
                        return this.m20;
                    case 9:
                        return this.m21;
                    case 10:
                        return this.m22;
                    case 11:
                        return this.m23;
                    case 12:
                        return this.m30;
                    case 13:
                        return this.m31;
                    case 14:
                        return this.m32;
                    case 15:
                        return this.m33;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        this.m00 = value;
                        break;
                    case 1:
                        this.m01 = value;
                        break;
                    case 2:
                        this.m02 = value;
                        break;
                    case 3:
                        this.m03 = value;
                        break;
                    case 4:
                        this.m10 = value;
                        break;
                    case 5:
                        this.m11 = value;
                        break;
                    case 6:
                        this.m12 = value;
                        break;
                    case 7:
                        this.m13 = value;
                        break;
                    case 8:
                        this.m20 = value;
                        break;
                    case 9:
                        this.m21 = value;
                        break;
                    case 10:
                        this.m22 = value;
                        break;
                    case 11:
                        this.m23 = value;
                        break;
                    case 12:
                        this.m30 = value;
                        break;
                    case 13:
                        this.m31 = value;
                        break;
                    case 14:
                        this.m32 = value;
                        break;
                    case 15:
                        this.m33 = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                return this.m00;
                            case 1:
                                return this.m01;
                            case 2:
                                return this.m02;
                            case 3:
                                return this.m03;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                return this.m10;
                            case 1:
                                return this.m11;
                            case 2:
                                return this.m12;
                            case 3:
                                return this.m13;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                    case 2:
                        switch (column)
                        {
                            case 0:
                                return this.m20;
                            case 1:
                                return this.m21;
                            case 2:
                                return this.m22;
                            case 3:
                                return this.m23;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                    case 3:
                        switch (column)
                        {
                            case 0:
                                return this.m30;
                            case 1:
                                return this.m31;
                            case 2:
                                return this.m32;
                            case 3:
                                return this.m33;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(row));
                }
            }

            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                this.m00 = value;
                                break;
                            case 1:
                                this.m01 = value;
                                break;
                            case 2:
                                this.m02 = value;
                                break;
                            case 3:
                                this.m03 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;
                    case 1:
                        switch (column)
                        {
                            case 0:
                                this.m10 = value;
                                break;
                            case 1:
                                this.m11 = value;
                                break;
                            case 2:
                                this.m12 = value;
                                break;
                            case 3:
                                this.m13 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;
                    case 2:
                        switch (column)
                        {
                            case 0:
                                this.m20 = value;
                                break;
                            case 1:
                                this.m21 = value;
                                break;
                            case 2:
                                this.m22 = value;
                                break;
                            case 3:
                                this.m23 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;
                    case 3:
                        switch (column)
                        {
                            case 0:
                                this.m30 = value;
                                break;
                            case 1:
                                this.m31 = value;
                                break;
                            case 2:
                                this.m32 = value;
                                break;
                            case 3:
                                this.m33 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(row));
                }
            }
        }

        public static Matrix4x4 zero
        {
            get
            {
                Matrix4x4 matrix = new Matrix4x4();
                matrix.m00 = 0; matrix.m01 = 0; matrix.m02 = 0; matrix.m03 = 0;
                matrix.m10 = 0; matrix.m11 = 0; matrix.m12 = 0; matrix.m13 = 0;
                matrix.m20 = 0; matrix.m21 = 0; matrix.m22 = 0; matrix.m23 = 0;
                matrix.m30 = 0; matrix.m31 = 0; matrix.m32 = 0; matrix.m33 = 0;
                return matrix;
            }
        }

        public static Matrix4x4 identity
        {
            get
            {
                Matrix4x4 matrix = new Matrix4x4();
                matrix.m00 = 1; matrix.m01 = 0; matrix.m02 = 0; matrix.m03 = 0;
                matrix.m10 = 0; matrix.m11 = 1; matrix.m12 = 0; matrix.m13 = 0;
                matrix.m20 = 0; matrix.m21 = 0; matrix.m22 = 1; matrix.m23 = 0;
                matrix.m30 = 0; matrix.m31 = 0; matrix.m32 = 0; matrix.m33 = 1;
                return matrix;
            }
        }

        public Matrix4x4 inverse
        {
            get
            {
                Matrix4x4 matrix = new Matrix4x4();
                matrix.m00 = this.m00; matrix.m01 = this.m10; matrix.m02 = this.m20; matrix.m03 = this.m30;
                matrix.m10 = this.m01; matrix.m11 = this.m11; matrix.m12 = this.m21; matrix.m13 = this.m31;
                matrix.m20 = this.m02; matrix.m21 = this.m12; matrix.m22 = this.m22; matrix.m23 = this.m32;
                matrix.m30 = this.m03; matrix.m31 = this.m13; matrix.m32 = this.m23; matrix.m33 = this.m33;
                return matrix;
            }
        }

        public static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = 2.0f / (right - left);
            matrix.m11 = 2.0f / (top - bottom);
            matrix.m22 = 2.0f / (zNear - zFar);
            matrix.m33 = 1.0f;
            matrix.m03 = -(right + left) / (right - left);
            matrix.m13 = -(top + bottom) / (top - bottom);
            matrix.m23 = -(zNear + zFar) / (zNear - zFar);
            return matrix;
        }

        internal static Matrix4x4 Frustrum(float left, float right, float bottom, float top, float near, float far)
        {
            if (Mathf.Approximately(right, left))
                throw new ArgumentException("left/right planes are coincident");
            if (Mathf.Approximately(top, bottom))
                throw new ArgumentException("top/bottom planes are coincident");
            if (Mathf.Approximately(far, near))
                throw new ArgumentException("far/near planes are coincident");

            Matrix4x4 matrix = new Matrix4x4();

            matrix.m00 = 2.0f * near / (right - left);
            matrix.m11 = 2.0f * near / (top - bottom);
            matrix.m02 = (right + left) / (right - left);
            matrix.m12 = (top + bottom) / (top - bottom);
            matrix.m22 = (-far - near) / (far - near);
            matrix.m32 = -1.0f;
            matrix.m23 = -2.0f * far * near / (far - near);

            return matrix;
        }

        public static Matrix4x4 Perspective(float fov, float aspect, float zNear, float zFar)
        {
            if (fov <= 0.0f || fov >= 180.0f)
                throw new ArgumentOutOfRangeException(nameof(fov), "not in range (0, 180)");
            if (Mathf.Approximately(zNear, 0))
                throw new ArgumentOutOfRangeException(nameof(zNear), "zero not allowed");
            if (Mathf.Abs(zFar) < Mathf.Abs(zNear))
                throw new ArgumentOutOfRangeException(nameof(zFar), "less than near");

            float ymax = zNear * (float)Math.Tan(fov / 2.0f * Mathf.Deg2Rad);
            float xmax = ymax * aspect;

            return Matrix4x4.Frustrum(-xmax, +xmax, -ymax, +ymax, zNear, zFar);
        }

        internal static Matrix4x4 Translated(Vector3 t)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m03 = t.x;
            matrix.m13 = t.y;
            matrix.m23 = t.z;
            return matrix;
        }

        public static Matrix4x4 Rotated(Quaternion r)
        {
            Matrix4x4 m1 = new Matrix4x4();
            m1.m00 = r.w; m1.m01 = r.z; m1.m02 = -r.y; m1.m03 = r.x;
            m1.m10 = -r.z; m1.m11 = r.w; m1.m12 = r.x; m1.m13 = r.y;
            m1.m20 = r.y; m1.m21 = -r.x; m1.m22 = r.w; m1.m23 = r.z;
            m1.m30 = -r.x; m1.m31 = -r.y; m1.m32 = -r.z; m1.m33 = r.w;

            Matrix4x4 m2 = new Matrix4x4();
            m2.m00 = r.w; m2.m01 = r.z; m2.m02 = -r.y; m2.m03 = -r.x;
            m2.m10 = -r.z; m2.m11 = r.w; m2.m12 = r.x; m2.m13 = -r.y;
            m2.m20 = r.y; m2.m21 = -r.x; m2.m22 = r.w; m2.m23 = -r.z;
            m2.m30 = r.x; m2.m31 = r.y; m2.m32 = r.z; m2.m33 = r.w;

            return m2 * m1;
        }

        internal static Matrix4x4 Scaled(Vector3 s)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m00 = s.x;
            matrix.m11 = s.y;
            matrix.m22 = s.z;
            return matrix;
        }

        internal void Translate(Vector3 t)
        {
            this = Matrix4x4.Translated(t) * this;
        }

        internal void Rotate(Quaternion r)
        {
            this = Matrix4x4.Rotated(r).inverse * this;
        }

        //public static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar);

        //public static Matrix4x4 Perspective(float fov, float aspect, float zNear, float zFar);

        internal void Scale(Vector3 s)
        {
            this = Matrix4x4.Scaled(s) * this;
        }

        public static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(pos, q, s);
            return matrix;
        }

        public void SetTRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            this = Matrix4x4.identity;
            this.Scale(s);
            this.Rotate(q);
            this.Translate(pos);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4x4))
            {
                return false;
            }

            var x = (Matrix4x4)obj;
            return m00 == x.m00 &&
                   m33 == x.m33 &&
                   m23 == x.m23 &&
                   m13 == x.m13 &&
                   m03 == x.m03 &&
                   m32 == x.m32 &&
                   m22 == x.m22 &&
                   m02 == x.m02 &&
                   m12 == x.m12 &&
                   m21 == x.m21 &&
                   m11 == x.m11 &&
                   m01 == x.m01 &&
                   m30 == x.m30 &&
                   m20 == x.m20 &&
                   m10 == x.m10 &&
                   m31 == x.m31;
        }

        public override int GetHashCode()
        {
            var hashCode = -647698992;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + m00.GetHashCode();
            hashCode = hashCode * -1521134295 + m33.GetHashCode();
            hashCode = hashCode * -1521134295 + m23.GetHashCode();
            hashCode = hashCode * -1521134295 + m13.GetHashCode();
            hashCode = hashCode * -1521134295 + m03.GetHashCode();
            hashCode = hashCode * -1521134295 + m32.GetHashCode();
            hashCode = hashCode * -1521134295 + m22.GetHashCode();
            hashCode = hashCode * -1521134295 + m02.GetHashCode();
            hashCode = hashCode * -1521134295 + m12.GetHashCode();
            hashCode = hashCode * -1521134295 + m21.GetHashCode();
            hashCode = hashCode * -1521134295 + m11.GetHashCode();
            hashCode = hashCode * -1521134295 + m01.GetHashCode();
            hashCode = hashCode * -1521134295 + m30.GetHashCode();
            hashCode = hashCode * -1521134295 + m20.GetHashCode();
            hashCode = hashCode * -1521134295 + m10.GetHashCode();
            hashCode = hashCode * -1521134295 + m31.GetHashCode();
            return hashCode;
        }

        public Vector4 GetRow(int i)
        {
            return new Vector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
        }

        public override string ToString()
        {
            return String.Format("({0},\n {1},\n {2},\n {3})", this.GetRow(0), this.GetRow(1), this.GetRow(2), this.GetRow(3));
        }

        public static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            matrix.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            matrix.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            matrix.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            matrix.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            matrix.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            matrix.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            matrix.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            matrix.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            matrix.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            matrix.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            matrix.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            matrix.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;
            matrix.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;
            matrix.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;
            matrix.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
            return matrix;
        }

        public static Vector4 operator *(Matrix4x4 lhs, Vector4 v)
        {
            return new Vector4(
                lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z + lhs.m03 * v.w,
                lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z + lhs.m13 * v.w,
                lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z + lhs.m23 * v.w,
                lhs.m30 * v.x + lhs.m31 * v.y + lhs.m32 * v.z + lhs.m33 * v.w
            );
        }

        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return
                lhs.m00 == rhs.m00 && lhs.m01 == rhs.m01 && lhs.m02 == rhs.m02 && lhs.m03 == rhs.m03 &&
                lhs.m10 == rhs.m10 && lhs.m11 == rhs.m11 && lhs.m12 == rhs.m12 && lhs.m13 == rhs.m13 &&
                lhs.m20 == rhs.m20 && lhs.m21 == rhs.m21 && lhs.m22 == rhs.m22 && lhs.m23 == rhs.m23 &&
                lhs.m30 == rhs.m30 && lhs.m31 == rhs.m31 && lhs.m32 == rhs.m32 && lhs.m33 == rhs.m33;
        }

        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }

        internal float[] ToArray()
        {
            return new float[] {
                this[0],  this[1],  this[2],  this[3],
                this[4],  this[5],  this[6],  this[7],
                this[8],  this[9],  this[10], this[11],
                this[12], this[13], this[14], this[15],};
        }

        internal float[] ToOpenGLStyleArray()
        {
            return this.inverse.ToArray();
        }
    }
}
