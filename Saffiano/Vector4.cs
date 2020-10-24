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

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector3 xyz => new Vector3(x, y, z);

        public Vector4(Vector3 v, float w) : this(v.x, v.y, v.z, w)
        {
        }

        public override string ToString()
        {
            return String.Format("({0:F2}, {1:F2}, {2:F2}, {3:F2})", this.x, this.y, this.z, this.w);
        }
    }
}
