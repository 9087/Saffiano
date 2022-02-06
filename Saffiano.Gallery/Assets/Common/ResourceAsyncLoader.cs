using System.Collections;

namespace Saffiano.Gallery.Assets.Components
{
    public class ResourceAsyncLoader : Behaviour
    {
        public string path = null;
        protected ResourceRequest resourceRequest;

        void Start()
        {
            this.StartCoroutine(this.Load());
        }

        public virtual IEnumerator Load()
        {
            yield return new WaitForSeconds(0.5f);
            this.resourceRequest = Resources.LoadAsync(this.path);
            yield return this.resourceRequest;
        }
    }
}
