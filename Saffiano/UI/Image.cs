using Saffiano.Rendering;

namespace Saffiano.UI
{
    public sealed class Image : Graphic
    {
        private Rect rect;

        public Sprite sprite { get; set; } = null;

        public Material material { get; set; } = new Resources.Default.Material.Basic();

        protected override Mesh OnPopulateMesh(Mesh old)
        {
            if (sprite == null || material == null || (this.mesh != null && this.rect == rectTransform.rect))
            {
                return old;
            }
            var @new = new Resources.Default.Mesh.Plane();
            this.rect = rectTransform.rect;
            old.vertices[0] = new Vector3(rect.left, rect.top);
            old.vertices[1] = new Vector3(rect.left, rect.bottom);
            old.vertices[2] = new Vector3(rect.right, rect.bottom);
            old.vertices[3] = new Vector3(rect.right, rect.top);
            foreach (var modifier in this.GetComponents<BaseMeshEffect>())
            {
                modifier.ModifyMesh(this.mesh);
            }
            return @new;
        }

        internal override Command GenerateCommand()
        {
            mesh = OnPopulateMesh(mesh);
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
