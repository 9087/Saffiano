using System.Collections.Generic;

namespace Saffiano
{
    public class Text : Graphic
    {
        private bool dirty = false;
        private string _text = string.Empty;
        private Font _font = null;
        private Rect rect;
        private Mesh mesh = null;

        public virtual string text
        {
            get => _text;

            set
            {
                _text = value;
                dirty = true;
            }
        }

        public Font font
        {
            get => _font;

            set
            {
                _font = value;
                dirty = true;
            }
        }

        public Material material { get; set; } = new Content.Material.Basic();

        internal override Command CreateCommand(RectTransform rectTransform)
        {
            if (material == null)
            {
                return null;
            }
            if (dirty)
            {
                _font.RequestCharactersInTexture(text);
            }
            if (this.rect != rectTransform.rect)
            {
                dirty = true;
            }
            if (dirty)
            {
                this.rect = rectTransform.rect;
                mesh = new Mesh() { primitiveType = PrimitiveType.Quads, };
                List<Vector3> vertices = new List<Vector3>();
                List<uint> indices = new List<uint>();
                List<Vector2> uv = new List<Vector2>();
                Vector2 current = Vector2.zero;
                Vector2 offset = new Vector2(rect.x, rect.y);
                uint index = 0;
                foreach (char ch in text)
                {
                    var characterInfo = font.GetCharacterInfo(ch);

                    if (characterInfo.texture == null)
                    {
                        current.x += characterInfo.advance.x;
                        continue;
                    }

                    float width = characterInfo.texture.width;
                    float height = characterInfo.texture.height;

                    float glyphX = current.x + offset.x;
                    float glyphY = current.y + offset.y + characterInfo.bitmapOffset.y - characterInfo.texture.height - characterInfo.descender;

                    float left = glyphX;
                    float right = glyphX + width;
                    float bottom = glyphY;
                    float top = glyphY + height;

                    vertices.Add(new Vector3(left, bottom));
                    vertices.Add(new Vector3(right, bottom));
                    vertices.Add(new Vector3(right, top));
                    vertices.Add(new Vector3(left, top));

                    uv.Add(characterInfo.uvBottomLeft);
                    uv.Add(characterInfo.uvBottomRight);
                    uv.Add(characterInfo.uvTopRight);
                    uv.Add(characterInfo.uvTopLeft);

                    indices.Add(index * 4 + 0);
                    indices.Add(index * 4 + 1);
                    indices.Add(index * 4 + 2);
                    indices.Add(index * 4 + 3);

                    index += 1;
                    current.x += characterInfo.advance.x;
                }
                mesh.vertices = vertices.ToArray();
                mesh.indices = indices.ToArray();
                mesh.uv = uv.ToArray();
                dirty = false;
            }
            return new Command()
            {
                projection = Rendering.projection,
                transform = rectTransform.ToRenderingMatrix(Rendering.device.coordinateSystem),
                mesh = mesh,
                mainTexture = Font.atlas,
                depthTest = false,
                lighting = false,
                blend = true,
                material = material,
            };
        }
    }
}
