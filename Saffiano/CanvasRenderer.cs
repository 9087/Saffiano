namespace Saffiano
{
    public sealed class CanvasRenderer : Renderer
    {
        private Graphic graphic => gameObject.GetComponent<Graphic>();

        private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

        private Mesh mesh = Mesh.plane;

        protected override void OnRender()
        {
            if (graphic == null || graphic.sprite == null)
            {
                return;
            }
            Rect rect = rectTransform.rect;
            mesh.vertices[0] = new Vector3(rect.left, rect.top);
            mesh.vertices[1] = new Vector3(rect.right, rect.top);
            mesh.vertices[2] = new Vector3(rect.right, rect.bottom);
            mesh.vertices[3] = new Vector3(rect.left, rect.bottom);
            var command = new Command();
            command.mesh = mesh;
            command.texture = graphic.sprite.texture;
            command.depthTest = false;
            command.lighting = false;
            Rendering.Draw(command);
        }
    }
}
