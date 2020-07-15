using System;
using System.Collections.Generic;

namespace Saffiano
{
    public abstract class ScriptingPrefab : Object
    {
        private static Dictionary<Type, ScriptingPrefab> caches = new Dictionary<Type, ScriptingPrefab>();

        internal static GameObject Load<T>() where T : ScriptingPrefab, new()
        {
            Type type = typeof(T);
            if (!caches.ContainsKey(type))
            {
                caches[type] = new T();
            }
            return caches[type].gameObject;
        }

        private GameObject gameObject = null;

        protected ScriptingPrefab()
        {
            gameObject = new GameObject() { target = Transform.background };
            Construct(gameObject);
            gameObject.transform.SetInternalParent(Transform.background);
        }

        public abstract void Construct(GameObject gameObject);
    }
}
