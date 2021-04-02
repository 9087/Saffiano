using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Saffiano
{
    public class Behaviour : Component
    {
        private static Dictionary<Type, Dictionary<string, MethodInfo>> methodInfoCollection = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        private Dictionary<object, List<Coroutine>> coroutines = new Dictionary<object, List<Coroutine>>();
        bool started = false;
        bool awaken = false;
        bool _enabled = false;

        public bool enabled
        {
            get => _enabled;

            set
            {
                if (_enabled == value)
                {
                    return;
                }
                _enabled = value;
                RequestEnableOrDisable();
            }
        }

        public bool isActiveAndEnabled => gameObject.activeInHierarchy && enabled;

        private void RequestEnableOrDisable()
        {
            var active = gameObject.activeInHierarchy;
            if (active && _enabled)
            {
                this.Invoke("OnEnable");
            }
            else if (!active && !_enabled)
            {
                this.Invoke("OnDisable");
            }
        }

        internal override void OnGameObjectActiveInHierarchyChanged(bool old, bool current)
        {
            base.OnGameObjectActiveInHierarchyChanged(old, current);
            if (current && !awaken)
            {
                this.Invoke("Awake");
                awaken = true;
            }
            if (old != current)
            {
                RequestEnableOrDisable();
            }
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            started = false;
            base.OnComponentAdded(gameObject);
            if (gameObject.activeInHierarchy && !awaken)
            {
                this.Invoke("Awake");
                awaken = true;
            }
            this.enabled = true;
        }

        internal override void OnComponentRemoved()
        {
            this.Invoke("OnDestroy");
            base.OnComponentRemoved();
        }

        internal void Invoke(string methodName, params object[] value)
        {
            Dictionary<string, MethodInfo> methodInfos;
            Type type = this.GetType();
            if (!methodInfoCollection.TryGetValue(this.GetType(), out methodInfos))
            {
                methodInfos = new Dictionary<string, MethodInfo>();
                methodInfoCollection.Add(type, methodInfos);
            }
            MethodInfo methodInfo;
            if (!methodInfos.TryGetValue(methodName, out methodInfo))
            {
                methodInfo = GetMethodInfo(methodName);
                methodInfos.Add(methodName, methodInfo);
            }
            methodInfo?.Invoke(this, value);
        }

        internal MethodInfo GetMethodInfo(string methodName)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = this.GetType();
            while (type != typeof(Behaviour))
            {
                var methodInfo = type.GetMethod(methodName, bindingFlags);
                if (methodInfo != null && methodInfo.DeclaringType.IsSubclassOf(typeof(Behaviour)))
                {
                    return methodInfo;
                }
                type = type.GetTypeInfo().BaseType;
            }
            return null;
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            Coroutine coroutine = new Coroutine(routine);
            coroutine.Start();
            int hash = routine.GetHashCode();
            if (!this.coroutines.ContainsKey(hash))
            {
                this.coroutines.Add(hash, new List<Coroutine>());
            }
            this.coroutines[routine.GetHashCode()].Add(coroutine);
            return coroutine;
        }

        public void StopCoroutine(IEnumerator routine)
        {
            int hash = routine.GetHashCode();
            if (!this.coroutines.ContainsKey(hash))
            {
                return;
            }
            foreach (Coroutine coroutine in this.coroutines[hash])
            {
                coroutine.Interrupt();
            }
            this.coroutines.Remove(hash);
        }

        internal void RequestUpdate()
        {
            if (!this.started)
            {
                this.Invoke("Start");
                this.started = true;
            }
            this.Invoke("Update");
        }
    }
}
