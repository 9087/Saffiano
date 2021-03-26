using Saffiano.FileFormats;

namespace Saffiano
{
    public class CharacterInfo
    {
        internal TTF ttf;

        public int index;

        internal Texture texture;

        internal Vector2 bitmapOffset;

        internal Vector2 advance;

        internal int ascender => ttf.face.Ascender;

        internal int descender => ttf.face.Descender;

        internal int lineSpacing => ttf.face.LineSpacing;

        public Vector2 uvBottomRight => this.texture.uvBottomRight;

        public Vector2 uvBottomLeft => this.texture.uvBottomLeft;

        public Vector2 uvTopRight => this.texture.uvTopRight;

        public Vector2 uvTopLeft => this.texture.uvTopLeft;

        internal CharacterInfo(TTF ttf, int index, Vector2 advance, Texture texture, Vector2 bitmapOffset)
        {
            this.ttf = ttf;
            this.index = index;
            this.texture = texture;
            this.bitmapOffset = bitmapOffset;
            this.advance = advance;
        }

        internal CharacterInfo(TTF ttf, int index, Vector2 advance)
        {
            this.ttf = ttf;
            this.index = index;
            this.texture = null;
            this.advance = advance;
        }

        ~CharacterInfo()
        {
            this.ttf = null;
            this.texture = null;
        }
    }
}
