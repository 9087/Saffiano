using Saffiano.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Saffiano.UI
{
    public enum TextAnchor
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        LowerLeft,
        LowerCenter,
        LowerRight,
    }

    public class Text : Graphic , ILayoutElement
    {
        private bool dirty = false;
        private string _text = string.Empty;
        private Font _font = null;
        private Rect rect;
        private TextAnchor _alignment = TextAnchor.MiddleCenter;
        private Vector2 preferredSize;

        private static Dictionary<TextAnchor, Vector2> alignments = new Dictionary<TextAnchor, Vector2>
        {
            {TextAnchor.UpperLeft,    new Vector2(0.0f, 1.0f)},
            {TextAnchor.UpperCenter,  new Vector2(0.5f, 1.0f)},
            {TextAnchor.UpperRight,   new Vector2(1.0f, 1.0f)},
            {TextAnchor.MiddleLeft,   new Vector2(0.0f, 0.5f)},
            {TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f)},
            {TextAnchor.MiddleRight,  new Vector2(1.0f, 0.5f)},
            {TextAnchor.LowerLeft,    new Vector2(0.0f, 0.0f)},
            {TextAnchor.LowerCenter,  new Vector2(0.5f, 0.0f)},
            {TextAnchor.LowerRight,   new Vector2(1.0f, 0.0f)},
        };

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

        public Material material { get; set; } = new Resources.Default.Material.Basic();

        public TextAnchor alignment
        {
            get => _alignment;

            set
            {
                _alignment = value;
                dirty = true;
            }
        }

        public float flexibleWidth => throw new NotImplementedException();

        public float flexibleHeight => throw new NotImplementedException();

        public float minWidth => throw new NotImplementedException();

        public float minHeight => throw new NotImplementedException();

        public float preferredHeight { get => preferredSize.y; }

        public float preferredWidth { get => preferredSize.x; }

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
                List<Vector3> vertices = new List<Vector3>();
                List<uint> indices = new List<uint>();
                List<Vector2> uv = new List<Vector2>();
                List<Color> colors = new List<Color>();
                Vector2 current = Vector2.zero;
                Vector2 offset = new Vector2(rect.x, rect.y);
                Vector2 lineSize = Vector2.zero;
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

                    lineSize.y = Mathf.Max(lineSize.y, characterInfo.ascender - characterInfo.descender);
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

                    colors.Add(new Color(1, 1, 1, 1));
                    colors.Add(new Color(1, 1, 1, 1));
                    colors.Add(new Color(1, 1, 1, 1));
                    colors.Add(new Color(1, 1, 1, 1));

                    index += 1;
                    current.x += characterInfo.advance.x;
                    lineSize.x = Mathf.Max(lineSize.x, current.x);
                }

                // TODO: multiple lines
                ;

                preferredSize = lineSize;
                Vector2 alignmentValue = alignments[alignment];
                var size = rectTransform.rect.size;
                var delta = (size - preferredSize) * alignmentValue;
                vertices = vertices
                    .Select((v) => v + new Vector3(delta.x, delta.y, 0))
                    .ToList();

                AutoLayout.MarkLayoutForRebuild(this.transform as RectTransform);

                mesh = new Mesh()
                {
                    primitiveType = PrimitiveType.Quads,
                    vertices = vertices.ToArray(),
                    indices = indices.ToArray(),
                    uv = uv.ToArray(),
                    colors = colors.ToArray()
                };
                foreach (var modifier in this.GetComponents<BaseMeshEffect>())
                {
                    modifier.ModifyMesh(this.mesh);
                }
                dirty = false;
            }
            return new Command()
            {
                projection = RenderPipeline.projection,
                transform = rectTransform.localToWorldMatrix,
                mesh = mesh,
                mainTexture = Font.atlas,
                depthTest = false,
                lighting = false,
                blend = true,
                material = material,
            };
        }

        public void CalculateLayoutInput()
        {
            return;
        }
    }
}
