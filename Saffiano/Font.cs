using Saffiano.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;

namespace Saffiano
{
    public class FontAtlas : Atlas
    {
        public FontAtlas(uint width, uint height) : base(width, height, spacing: 2)
        {
        }
    }

    public class Font : Object
    {
        private struct Identity : IEquatable<Identity>
        {
            public string fontname { get; private set; }

            public int size { get; private set; }

            public Identity(string fontname, int size)
            {
                this.fontname = fontname;
                this.size = size;
            }

            public bool Equals(Identity other)
            {
                return this.fontname == other.fontname && this.size == other.size;
            }
        }

        private static Dictionary<Identity, Font> instances = new Dictionary<Identity, Font>();

        public static FontAtlas atlas { get; } = new FontAtlas(1024, 1024);

        private Dictionary<char, CharacterInfo> characterInfos = new Dictionary<char, CharacterInfo>();

        private Font() : base()
        {
        }

        internal TTF[] _ttfs;

        private string[] _fontNames;

        public string[] fontNames
        { 
            get => _fontNames;

            set
            {
                _fontNames = value;
                _ttfs = new TTF[_fontNames.Length];
                for (int i = 0; i < _fontNames.Length; i++)
                {
                    _ttfs[i] = new TTF(_fontNames[i], (uint) fontSize);
                }
            }
        }

        public float lineHeight => ascender - descender;

        public float ascender => _ttfs[0].face.Ascender;

        public float descender => _ttfs[0].face.Descender;

        public int fontSize { get; private set; } = 14;

        public static Font CreateDynamicFontFromOSFont(string fontname, int size)
        {
            fontname = Path.Combine(Resources.rootDirectory, fontname);
            Identity identity = new Identity(fontname, size);
            if (instances.TryGetValue(identity, out Font font))
            {
                return font;
            }
            font = new Font();
            font.fontSize = size;
            var fallback = Path.Combine(Resources.rootDirectory, "fonts/simsun.ttc");
            font.fontNames = new string[] { fontname, fallback };
            instances.Add(identity, font);
            return font;
        }

        public CharacterInfo GetCharacterInfo(char ch)
        {
            if (characterInfos.TryGetValue(ch, out CharacterInfo value))
            {
                return value;
            }
            return null;
        }

        public void RequestCharactersInTexture(string characters)
        {
            foreach (var ch in characters)
            {
                if (characterInfos.ContainsKey(ch))
                {
                    continue;
                }
                foreach (var ttf in _ttfs)
                {
                    var characterInfo = ttf.RequestCharacterInfo(ch);
                    if (characterInfo == null)
                    {
                        continue;
                    }
                    characterInfos.Add(ch, characterInfo);
                    break;
                }
            }
        }
    }
}
