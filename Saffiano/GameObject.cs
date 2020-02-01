using System;
using System.Collections.Generic;
using System.Reflection;

namespace Saffiano
{
    public sealed class GameObject : Object
    {
        private List<Component> components = new List<Component>();

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
            return this.components.FindAll(c => c is T).ToArray() as T[];
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
                if (behaviour == null)
                {
                    continue;
                }
                try
                {
                    behaviour.RequestUpdate();
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogErrorFormat("Exception thrown: {0}\n{1}", tie.InnerException.ToString(), tie.InnerException.StackTrace);
                }
            }
            foreach (Transform transform in this.transform.children)
            {
                transform.gameObject.RequestUpdate();
            }
        }
    }
}
