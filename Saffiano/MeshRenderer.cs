namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter => gameObject.GetComponent<MeshFilter>();

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null)
            {
                return;
            }
            var command = new Command();
            command.mesh = meshFilter.mesh;
            Rendering.Draw(command);
        }
    }
}
