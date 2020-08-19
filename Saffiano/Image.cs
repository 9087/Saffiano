namespace Saffiano
{
    public sealed class Image : Graphic
    {
        private static GPUProgram shader = new GPUProgram("../../../../Resources/shader/normal.vs", "../../../../Resources/shader/normal.fs");

        private Rect rect;
        private Mesh mesh = null;

        public Sprite sprite { get; set; } = null;

        internal override Command CreateCommand(RectTransform rectTransform)
        {
            if (sprite == null)
            {
                return null;
            }
            if (this.mesh == null || this.rect != rectTransform.rect)
            {
                this.mesh = Mesh.plane;
                this.rect = rectTransform.rect;
                mesh.vertices[0] = new Vector3(rect.left, rect.top);
                mesh.vertices[1] = new Vector3(rect.right, rect.top);
                mesh.vertices[2] = new Vector3(rect.right, rect.bottom);
                mesh.vertices[3] = new Vector3(rect.left, rect.bottom);
            }
            return new Command()
            {
                projection = Rendering.projection,
                transform = rectTransform.ToRenderingMatrix(Rendering.device.coordinateSystem),
                mesh = this.mesh,
                texture = this.sprite.texture,
                depthTest = false,
                lighting = false,
                blend = true,
                shader = shader,
            };
        }
    }
}
