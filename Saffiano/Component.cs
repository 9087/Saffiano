namespace Saffiano
{
    public class Component : Object
    {
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
    }
}