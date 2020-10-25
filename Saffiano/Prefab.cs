using System;
using System.Collections.Generic;

namespace Saffiano
{
    public abstract class ScriptablePrefab : Object
    {
        private static Dictionary<Type, ScriptablePrefab> caches = new Dictionary<Type, ScriptablePrefab>();

        internal static GameObject Load<T>() where T : ScriptablePrefab, new()
        {
            Type type = typeof(T);
            if (!caches.ContainsKey(type))
            {
                caches[type] = new T();
            }
            return caches[type].gameObject;
        }

        private GameObject gameObject = null;

        protected ScriptablePrefab()
        {
            gameObject = new GameObject() { target = Transform.background };
            Construct(gameObject);
            gameObject.transform.SetInternalParent(Transform.background);
        }

        public abstract void Construct(GameObject gameObject);
    }
}
