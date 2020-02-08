using System;
using System.Collections.Generic;

namespace Saffiano
{
    internal abstract class Cache<TKey, TValue> : Dictionary<TKey, TValue>
    {
        protected abstract TValue OnRegister(TKey key);

        protected abstract void OnUnregister(TKey key);

        public TValue Register(TKey key)
        {
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

        public void Keep(ICollection<TKey> keys)
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
            if (ContainsKey(key))
            {
                return this[key];
            }
            return Register(key);
        }
    }
}
