﻿using System;

namespace Saffiano
{
    public struct Rect : IEquatable<Rect>
    {
        public Vector2 position { get; set; }

        public Vector2 size { get; set; }

        public float x
        {
            get
            {
                return position.x;
            }
            set
            {
                position = new Vector2(value, position.y);
            }
        }

        public float y
        {
            get
            {
                return position.y;
            }
            set
            {
                position = new Vector2(position.x, value);
            }
        }

        public float width
        {
            get
            {
                return size.x;
            }
            set
            {
                size = new Vector2(value, size.y);
            }
        }

        public float height
        {
            get
            {
                return size.y;
            }
            set
            {
                size = new Vector2(size.x, value);
            }
        }

        public float left
        {
            get
            {
                return position.x;
            }
            set
            {
                float right = this.right;
                position = new Vector2(value, position.y);
                size = new Vector2(right - value, size.y);
            }
        }

        public float right
        {
            get
            {
                return position.x + size.x;
            }
            set
            {
                size = new Vector2(value - position.x, size.y);
            }
        }

        public float bottom
        {
            get
            {
                return position.y;
            }
            set
            {
                float top = this.top;
                position = new Vector2(position.x, value);
                size = new Vector2(size.x, top - value);
            }
        }

        public float top
        {
            get
            {
                return position.y + size.y;
            }
            set
            {
                size = new Vector2(size.x, value - position.y);
            }
        }

        public Rect(float x, float y, float width, float height) : this(new Vector2(x, y), new Vector2(width, height))
        {
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }

        public static Rect zero => new Rect(0, 0, 0, 0);

        public bool Equals(Rect other)
        {
            return this.x == other.x && this.y == other.y && this.width == other.width && this.height == other.height;
        }

        public override bool Equals(object other)
        {
            if (!(other is Rect))
            {
                return false;
            }
            return this.Equals((Rect)other);
        }

        public static bool operator ==(Rect a, Rect b) => a.Equals(b);

        public static bool operator !=(Rect a, Rect b) => !a.Equals(b);

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.width, this.height);
        }

        public override int GetHashCode()
        {
            var hashCode = 1804577526;
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            return hashCode;
        }
    }
}
