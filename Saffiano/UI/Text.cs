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
        private bool dirty = true;
        private string _text = string.Empty;
        private Font _font = null;
        private Rect rect;
        private TextAnchor _alignment = TextAnchor.MiddleCenter;
        private Vector2 preferredSize;

        internal delegate void MeshPopulatedChangedHandler(Mesh mesh);
        internal MeshPopulatedChangedHandler MeshPopulatedChanged;

        internal Vector2[] carets { get; set; }

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
                if (_text == value)
                {
                    return;
                }
                _text = value;
                SetDirty();
            }
        }

        public Font font
        {
            get => _font;

            set
            {
                if (_font == value)
                {
                    return;
                }
                _font = value;
                SetDirty();
            }
        }

        public Material material { get; set; } = new Resources.Default.Material.Basic()
        {
            zTest = ZTest.Always,
            blend = Blend.transparency,
        };

        public TextAnchor alignment
        {
            get => _alignment;

            set
            {
                if (_alignment == value)
                {
                    return;
                }
                _alignment = value;
                SetDirty();
            }
        }

        public float flexibleWidth => throw new NotImplementedException();

        public float flexibleHeight => throw new NotImplementedException();

        public float minWidth => throw new NotImplementedException();

        public float minHeight => throw new NotImplementedException();

        public float preferredHeight
        {
            get
            {
                mesh = OnPopulateMesh(mesh);
                return preferredSize.y;
            }
        }

        public float preferredWidth
        {
            get
            {
                mesh = OnPopulateMesh(mesh);
                return preferredSize.x;
            }
        }

        private void SetDirty()
        {
            dirty = true;
            AutoLayout.MarkLayoutForRebuild(this.transform as RectTransform);
        }

        protected override Mesh OnPopulateMesh(Mesh old)
        {
            if (material == null)
            {
                return old;
            }
            if (dirty)
            {
                _font.RequestCharactersInTexture(text);
            }
            if (this.rect != rectTransform.rect)
            {
                dirty = true;
            }
            if (!dirty)
            {
                return old;
            }
            this.rect = rectTransform.rect;
            List<Vector3> vertices = new List<Vector3>();
            List<uint> indices = new List<uint>();
            List<Vector2> uv = new List<Vector2>();
            List<Color> colors = new List<Color>();
            Vector2 current = Vector2.zero;
            Vector2 offset = new Vector2(rect.left, rect.top);
            Vector2 size = Vector2.zero;
            List<Vector2> carets = new List<Vector2>() { new Vector2(0, -font.lineHeight) };
            uint index = 0;
            foreach (char ch in text)
            {
                var characterInfo = font.GetCharacterInfo(ch);

                if (characterInfo == null)
                {
                    carets.Add(current + new Vector2(0, -font.lineHeight));
                    continue;
                }

                if (characterInfo.texture == null)
                {
                    current.x += characterInfo.advance.x;
                    size.x = Mathf.Max(size.x, current.x);
                    carets.Add(current + new Vector2(0, -font.lineHeight));
                    continue;
                }

                var target = current.x + characterInfo.advance.x;
                if (target > rect.width)
                {
                    current.x = 0;
                    current.y -= font.lineHeight;
                }

                float width = characterInfo.texture.width;
                float height = characterInfo.texture.height;

                float glyphX = current.x + offset.x + characterInfo.bitmapOffset.x;
                float glyphY = current.y + offset.y - height - characterInfo.lineSpacing - characterInfo.descender + characterInfo.bitmapOffset.y;

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

                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);

                index += 1;
                current.x += characterInfo.advance.x;
                size.x = Mathf.Max(size.x, current.x);

                carets.Add(current + new Vector2(0, -font.lineHeight));
            }

            preferredSize = new Vector2(size.x, Mathf.Abs(current.y - font.lineHeight));
            Vector2 alignmentValue = alignments[alignment];
            var delta = (rectTransform.rect.size - preferredSize) * (alignmentValue + new Vector2(0, -1));
            vertices = vertices
                .Select((v) => v + new Vector3(delta.x, delta.y, 0))
                .ToList();
            this.carets = carets.Select((v) => v + delta).ToArray();

            AutoLayout.MarkLayoutForRebuild(this.transform as RectTransform);

            var @new = new Mesh()
            {
                primitiveType = PrimitiveType.Quads,
                vertices = vertices.ToArray(),
                indices = indices.ToArray(),
                uv = uv.ToArray(),
                colors = colors.ToArray()
            };
            foreach (var modifier in this.GetComponents<BaseMeshEffect>())
            {
                modifier.ModifyMesh(@new);
            }
            MeshPopulatedChanged?.Invoke(@new);
            dirty = false;
            return @new;
        }

        internal override Command GenerateCommand()
        {
            mesh = OnPopulateMesh(mesh);
            return new Command()
            {
                projection = RenderPipeline.projection,
                transform = rectTransform.localToWorldMatrix,
                mesh = this.mesh,
                mainTexture = Font.atlas,
                lighting = false,
                material = material,
            };
        }

        protected void OnTransformParentChanged()
        {
            SetDirty();
        }

        public void CalculateLayoutInput()
        {
            return;
        }
    }
}
