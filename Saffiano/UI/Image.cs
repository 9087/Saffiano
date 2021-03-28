using Saffiano.Rendering;

namespace Saffiano.UI
{
    public sealed class Image : Graphic
    {
        private Rect rect;

        public Sprite sprite { get; set; } = null;

        public Material material { get; set; } = new Resources.Default.Material.Basic();

        internal override Command CreateCommand(RectTransform rectTransform)
        {
            if (sprite == null || material == null)
            {
                return null;
            }
            if (this.mesh == null || this.rect != rectTransform.rect)
            {
                this.mesh = new Resources.Default.Mesh.Plane();
                this.rect = rectTransform.rect;
                mesh.vertices[0] = new Vector3(rect.left, rect.top);
                mesh.vertices[1] = new Vector3(rect.left, rect.bottom);
                mesh.vertices[2] = new Vector3(rect.right, rect.bottom);
                mesh.vertices[3] = new Vector3(rect.right, rect.top);
                foreach (var modifier in this.GetComponents<BaseMeshEffect>())
                {
                    modifier.ModifyMesh(this.mesh);
                }
            }
            return new Command()
            {
                projection = RenderPipeline.projection,
                transform = rectTransform.localToWorldMatrix,
                mesh = this.mesh,
                mainTexture = this.sprite.texture,
                depthTest = false,
                lighting = false,
                blend = true,
                material = material,
            };
        }
    }
}
