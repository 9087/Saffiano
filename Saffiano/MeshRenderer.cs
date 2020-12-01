using Saffiano.Rendering;

namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter => gameObject.GetComponent<MeshFilter>();

        public Material material { get; set; } = new Resources.Default.Material.Phong();

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null || material == null)
            {
                return;
            }
            var command = new Command()
            {
                projection = RenderPipeline.projection,
                transform = transform.localToWorldMatrix,
                mesh = meshFilter.mesh,
                material = material,
            };
            RenderPipeline.Draw(command);
        }
    }
}
