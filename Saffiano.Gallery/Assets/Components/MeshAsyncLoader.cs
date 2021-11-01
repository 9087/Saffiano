using System.Collections;

namespace Saffiano.Gallery.Assets.Components
{
    public class MeshAsyncLoader : ResourceAsyncLoader
    {
        public override IEnumerator Load()
        {
            yield return base.Load();
            this.GetComponent<MeshFilter>().mesh = this.resourceRequest.asset as Mesh;
            this.resourceRequest = null;
        }
    }
}
