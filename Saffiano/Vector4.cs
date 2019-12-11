using System;

namespace Saffiano
{
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

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.z, this.w);
        }
    }
}
