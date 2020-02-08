using System;

namespace Saffiano
{
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

        public static Vector2 zero
        {
            get
            {
                return new Vector2(0, 0);
            }
        }

        public static Vector2 operator+(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator-(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public override String ToString()
        {
            return String.Format("({0}, {1})", this.x, this.y);
        }
    }
}
