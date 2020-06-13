using System.Collections.Generic;

namespace Saffiano
{
    internal class FontAtlas : Atlas
    {
        public FontAtlas(uint width, uint height) : base(width, height, spacing: 2)
        {
        }
    }

    public class Font : Object
    {
        internal static FontAtlas atlas = new FontAtlas(128, 128);
        private Dictionary<char, CharacterInfo> characterInfos = new Dictionary<char, CharacterInfo>();

        public Font(string name) : base()
        {
            this.name = name;
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

        public int fontSize { get; private set; } = 14;

        public static Font CreateDynamicFontFromOSFont(string fontname, int size)
        {
            var font = new Font(fontname);
            font.fontSize = size;
            font.fontNames = new string[] { fontname };
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
                }
            }
        }
    }
}
