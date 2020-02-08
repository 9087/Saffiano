namespace Saffiano
{
    internal class Command
    {
        public Mesh mesh { get; set; } = null;

        public Texture texture { get; set; } = null;

        public bool lighting { get; set; } = true;

        public bool depthTest { get; set; } = true;

        public Command()
        {
        }
    }
}
