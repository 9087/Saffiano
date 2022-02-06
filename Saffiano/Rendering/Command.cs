namespace Saffiano.Rendering
{
    internal class Command
    {
        public Matrix4x4 projection { get; set; }

        public Matrix4x4 transform { get; set; }

        public Mesh mesh { get; set; } = null;

        public Texture mainTexture { get; set; } = null;

        public bool lighting { get; set; } = true;

        public Material material { get; set; } = null;

        public Command()
        {
        }
    }
}
