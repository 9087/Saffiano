using System;

namespace Saffiano
{
    public class Asset : Object
    {
        public Guid id
        {
            get;
            private set;
        }

        public Asset()
        {
            id = System.Guid.NewGuid();
        }
    }
}
