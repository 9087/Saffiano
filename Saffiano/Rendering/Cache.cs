using System;
using System.Collections.Generic;

namespace Saffiano.Rendering
{
    internal interface IDeperactedClean
    {
        void Start();

        void End();

        void SetDevice(Device device);
    }

    internal abstract class Cache<TKey, TValue> : Dictionary<TKey, TValue>, IDeperactedClean
    {
        HashSet<TKey> visited = new HashSet<TKey>();

        protected abstract TValue OnRegister(TKey key);

        protected abstract void OnUnregister(TKey key);

        protected Device device { get; set; }

        public Cache()
        {
            this.device = null;
        }

        public void SetDevice(Device device)
        {
            this.device = device;
        }

        public TValue Register(TKey key)
        {
            visited.Add(key);
            if (ContainsKey(key))
            {
                throw new Exception();
            }
            var value = OnRegister(key);
            Add(key, value);
            return value;
        }

        public void Unregister(TKey key)
        {
            if (!ContainsKey(key))
            {
                throw new Exception();
            }
            OnUnregister(key);
            Remove(key);
        }

        public void UnregisterAll()
        {
            foreach (var key in new List<TKey>(Keys))
            {
                Unregister(key);
            }
        }

        protected void Keep(ICollection<TKey> keys)
        {
            List<TKey> discardedList = new List<TKey>();
            foreach (var key in Keys)
            {
                if (!keys.Contains(key))
                {
                    discardedList.Add(key);
                }
            }
            foreach (var key in discardedList)
            {
                Unregister(key);
            }
        }

        public TValue TryRegister(TKey key)
        {
            visited.Add(key);
            if (ContainsKey(key))
            {
                return this[key];
            }
            return Register(key);
        }

        public void Start()
        {
            visited.Clear();
        }

        public void End()
        {
            this.Keep(visited);
            visited.Clear();
        }
    }
}
