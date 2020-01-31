using System;

namespace Saffiano
{
    public class Object
    {
        public string name { get; set; } = "Unnamed";

        public Object()
        {
        }

        public override string ToString()
        {
            return string.Format("({0}: {1})", this.GetType().Name, this.name);
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
