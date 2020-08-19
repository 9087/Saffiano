namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter => gameObject.GetComponent<MeshFilter>();

        public GPUProgram shader = null;

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null || shader == null)
            {
                return;
            }
            var command = new Command()
            {
                projection = Rendering.projection,
                transform = transform.ToRenderingMatrix(Rendering.device.coordinateSystem),
                mesh = meshFilter.mesh,
                shader = shader,
            };
            Rendering.Draw(command);
        }
    }
}
