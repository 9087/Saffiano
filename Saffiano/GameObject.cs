using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saffiano
{
    public sealed class GameObject : Object
    {
        private List<Component> components = new List<Component>();

        public bool activeSelf { get; private set; } = true;

        public bool activeInHierarchy { get; private set; } = true;

        public void SetActive(bool active)
        {
            this.activeSelf = active;
        }

        private void UpdateActiveInHierarchyState(bool parent)
        {
            bool old = activeInHierarchy;
            activeInHierarchy = activeSelf && parent;
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
            UpdateActiveInHierarchyState(current == null ? true : current.gameObject.activeInHierarchy);
        }

        public Transform transform
        {
            get
            {
                return this.GetComponent<Transform>();
            }
        }

        public GameObject()
        {
        }

        public GameObject(String name)
        {
            this.name = name;
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

        public T GetComponent<T>() where T : Component
        {
            return this.components.Find(c => c is T) as T;
        }

        public T[] GetComponents<T>() where T : Component
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
            foreach (Component component in this.components)
            {
                Behaviour behaviour = component as Behaviour;
                if (behaviour == null || !behaviour.enabled)
                {
                    continue;
                }
                try
                {
                    behaviour.RequestUpdate();
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogException(tie);
                }
            }
            foreach (Transform transform in this.transform.children)
            {
                if (!transform.gameObject.activeInHierarchy)
                {
                    continue;
                }
                transform.gameObject.RequestUpdate();
            }
        }
    }
}
