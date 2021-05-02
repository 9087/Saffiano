using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saffiano
{
    public class GameObject : Object
    {
        private List<Component> components = new List<Component>();

        internal Transform target { get; set; } = Transform.scene;

        public bool activeSelf { get; private set; } = true;

        public bool activeInHierarchy { get; private set; } = false;

        public int layer { get; set; } = LayerMask.NameToLayer("Default");

        public void SetActive(bool active)
        {
            if (this.activeSelf == active)
            {
                return;
            }
            this.activeSelf = active;
            UpdateActiveInHierarchyState();
        }

        internal void UpdateActiveInHierarchyState()
        {
            var internalParent = this.transform.GetInternalParent();
            UpdateActiveInHierarchyState(internalParent == null ? true : internalParent.gameObject.activeInHierarchy);
        }

        private void UpdateActiveInHierarchyState(bool internalParentActiveInHierarchy)
        {
            bool old = activeInHierarchy;
            activeInHierarchy = activeSelf && internalParentActiveInHierarchy;
            if (activeInHierarchy == old)
            {
                return;
            }
            OnActiveInHierarchyChanged(old, activeInHierarchy);
            foreach (Transform child in transform)
            {
                child.gameObject.UpdateActiveInHierarchyState(activeInHierarchy);
            }
        }

        private void OnActiveInHierarchyChanged(bool old, bool current)
        {
            foreach (var component in components)
            {
                component.OnGameObjectActiveInHierarchyChanged(old, current);
            }
        }

        internal void OnParentChanged(Transform old, Transform current)
        {
        }

        internal void OnInternalParentChanged(Transform old, Transform current)
        {
            UpdateActiveInHierarchyState();
        }

        public Transform transform
        {
            get
            {
                return this.GetComponent<Transform>();
            }
        }

        public GameObject(String name)
        {
            this.name = name;
        }

        public GameObject()
        {
            this.name = GetType().Name;
        }

        private void AddComponent(Component component)
        {
            this.components.Add(component);
            component.OnComponentAdded(this);
        }

        internal void RemoveComponent(Component component)
        {
            component.OnComponentRemoved();
            this.components.Remove(component);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            this.AddComponent(component);
            return component;
        }

        public Component AddComponent(Type componentType)
        {
            Component component = (Component)Activator.CreateInstance(componentType);
            this.AddComponent(component);
            return component;
        }

        public T GetComponent<T>() where T : class
        {
            return this.components.Find(c => c is T) as T;
        }

        public T[] GetComponents<T>() where T : class
        {
            return this.components.FindAll(c => c is T).Select((c) => c as T).ToArray();
        }

        internal override void RequestDestroy()
        {
            foreach (Component component in this.components)
            {
                Object.Destroy(component);
            }
            foreach (Component component in this.components.ToArray())
            {
                this.components.Remove(component);
            }
        }

        internal void RequestUpdate()
        {
            foreach (var x in EnumerateBehaviours().Where((x) => x.enabled))
            {
                x.RequestUpdate();
            }
            foreach (var child in transform)
            {
                child.gameObject.RequestUpdate();
            }
        }

        internal void RequestLateUpdate()
        {
            foreach (var x in EnumerateBehaviours().Where((x) => x.enabled))
            {
                x.RequestLateUpdate();
            }
            foreach (var child in transform)
            {
                child.gameObject.RequestLateUpdate();
            }
        }

        public void SendMessage(string methodName, params object[] value)
        {
            if (!activeInHierarchy)
            {
                return;
            }
            foreach (var behaviour in EnumerateBehaviours())
            {
                try
                {
                    behaviour.Invoke(methodName, value);
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogException(tie);
                }
            }
        }

        public void BroadcastMessage(string methodName, params object[] value)
        {
            if (!activeInHierarchy)
            {
                return;
            }
            SendMessage(methodName, value);
            foreach (Transform transform in this.transform.children)
            {
                transform.gameObject.BroadcastMessage(methodName, value);
            }
        }

        private IEnumerable<Behaviour> EnumerateBehaviours()
        {
            var components = this.components.ToList();
            foreach (Component component in components)
            {
                if (!(component is Behaviour))
                {
                    continue;
                }
                yield return component as Behaviour;
            }
        }
    }
}
