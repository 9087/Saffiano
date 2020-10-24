namespace Saffiano
{
    internal class Command
    {
        public Matrix4x4 projection { get; set; }

        public Matrix4x4 transform { get; set; }

        public Mesh mesh { get; set; } = null;

        public Texture texture { get; set; } = null;

        public bool lighting { get; set; } = true;

        public bool depthTest { get; set; } = true;

        public bool blend { get; set; } = false;

        public Material material { get; set; } = null;

        public Command()
        {
        }
    }
}
