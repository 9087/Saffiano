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

        public T GetComponent<T>() where T : Component
        {
            return this.gameObject.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return this.gameObject.GetComponents<T>();
        }

        internal virtual void OnGameObjectActiveInHierarchyChanged(bool old, bool current)
        {
        }
    }
}