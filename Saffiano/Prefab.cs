using System;
using System.Collections.Generic;

namespace Saffiano
{
    public class Prefab : Object
    {
        private static Dictionary<Type, Prefab> caches = new Dictionary<Type, Prefab>();

        internal static GameObject Load<T>() where T : Prefab, new()
        {
            Type type = typeof(T);
            if (!caches.ContainsKey(type))
            {
                caches[type] = new T();
            }
            return caches[type].gameObject;
        }

        protected GameObject gameObject = null;
    }

    public abstract class ScriptablePrefab<T> : Prefab where T : GameObject, new()
    {
        protected ScriptablePrefab()
        {
            gameObject = new T() { target = Transform.background };
            Construct(gameObject as T);
            gameObject.transform.SetInternalParent(Transform.background);
        }

        public abstract void Construct(T gameObject);
    }
}
