using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public class LinearLayoutGroup : LayoutGroup
    {
        public new RectTransform transform => base.transform as RectTransform;

        private RectTransform.Axis _axis = RectTransform.Axis.Vertical;

        public RectTransform.Axis axis
        {
            get => _axis;
            set { _axis = value; }
        }

        private float _spacing = 0;

        public float spacing
        {
            get => _spacing;
            set
            {
                _spacing = value;
            }
        }

        public override void CalculateLayoutInput()
        {
            Vector2 size = Vector2.zero;
            foreach (RectTransform child in transform)
            {
                if (child == null)
                {
                    continue;
                }
                var layoutElement = child.GetComponent<ILayoutElement>();
                size.x += layoutElement.preferredWidth;
                size.y += layoutElement.preferredHeight;
            }
            preferredWidth = size.x;
            preferredHeight = size.y;
        }

        public override void SetLayout()
        {
            var rect = this.transform.rect;
            float step = 0;
            foreach (RectTransform child in transform)
            {
                if (child == null)
                {
                    continue;
                }
                var childRect = child.rect;
                child.pivot = new Vector2(0.5f, 0.5f);
                switch(_axis)
                {
                    case RectTransform.Axis.Horizontal:
                        child.anchorMin = new Vector2(0, 1);
                        child.anchorMax = new Vector2(0, 1);
                        child.offsetMin = new Vector2(step, -childRect.height);
                        child.offsetMax = new Vector2(childRect.width + step, 0);
                        step += childRect.width;
                        break;
                    case RectTransform.Axis.Vertical:
                        child.anchorMin = new Vector2(0, 1);
                        child.anchorMax = new Vector2(0, 1);
                        child.offsetMin = new Vector2(0, -childRect.height - step);
                        child.offsetMax = new Vector2(childRect.width, -step);
                        step += childRect.height;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                step += _spacing;
            }
        }
    }
}
