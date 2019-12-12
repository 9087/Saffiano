using System;

namespace Saffiano
{
    public class Object
    {
        public String name { get; set; }

        public Object()
        {
        }

        public override String ToString()
        {
            return String.Format("({0}: {1})", this.GetType().Name, this.GetHashCode());
        }

        public static void Destroy(Object obj)
        {
            obj.RequestDestroy();
        }

        internal virtual void RequestDestroy()
        {
        }
    }
}
