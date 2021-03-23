namespace Saffiano
{
    public class Component : Object
    {
        public Transform transform
        {
            get
            {
                return this.gameObject.transform;
            }
        }

        public GameObject gameObject
        {
            get;
            private set;
        }

        internal virtual void OnComponentAdded(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        internal virtual void OnComponentRemoved()
        {
            this.gameObject = null;
        }

        internal override void RequestDestroy()
        {
            this.gameObject.RemoveComponent(this);
        }

        public T GetComponent<T>() where T : class
        {
            return this.gameObject.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : class
        {
            return this.gameObject.GetComponents<T>();
        }

        internal virtual void OnGameObjectActiveInHierarchyChanged(bool old, bool current)
        {
        }

        public override string ToString()
        {
            return string.Format("({0}: {1})", this.GetType().Name, this.gameObject.name);
        }
    }
}