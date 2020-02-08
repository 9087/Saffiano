namespace Saffiano
{
    public sealed class Sprite : Object
    {
        public Texture texture
        {
            get;
            private set;
        }

        public static Sprite Create(Texture texture)
        {
            return new Sprite() { texture = texture };
        }
    }
}
