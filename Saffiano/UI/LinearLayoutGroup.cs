using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.UI
{
    public class LinearLayoutGroup : LayoutGroup
    {
        public new RectTransform transform => base.transform as RectTransform;

        private bool _childControlHeight = true;

        private bool _childControlWidth = true;

        public bool childControlHeight
        {
            get => _childControlHeight;
            set
            {
                if (_childControlHeight == value)
                {
                    return;
                }
                _childControlHeight = value;
                SetDirty();
            }
        }

        public bool childControlWidth
        {
            get => _childControlWidth;
            set
            {
                if (_childControlWidth == value)
                {
                    return;
                }
                _childControlWidth = value;
                SetDirty();
            }
        }

        private RectTransform.Axis _axis = RectTransform.Axis.Vertical;

        public RectTransform.Axis axis
        {
            get => _axis;
            set 
            {
                if (_axis == value)
                {
                    return;
                }
                _axis = value;
                SetDirty();
            }
        }

        private float _spacing = 0;

        public float spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value)
                {
                    return;
                }
                _spacing = value;
                SetDirty();
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
                switch (_axis)
                {
                    case RectTransform.Axis.Horizontal:
                        size.x += layoutElement.preferredWidth;
                        size.y = Mathf.Max(size.y, layoutElement.preferredHeight);
                        break;
                    case RectTransform.Axis.Vertical:
                        size.y += layoutElement.preferredHeight;
                        size.x = Mathf.Max(size.x, layoutElement.preferredWidth);
                        break;
                }
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
                child.pivot = new Vector2(0, 1);
                var layoutElement = child.GetComponent<ILayoutElement>();
                var width = _childControlWidth ? transform.rect.width : layoutElement.preferredWidth;
                var height = _childControlHeight ? transform.rect.height : layoutElement.preferredHeight;
                switch (_axis)
                {
                    case RectTransform.Axis.Horizontal:
                        child.anchorMin = new Vector2(0, 1);
                        child.anchorMax = new Vector2(0, 1);
                        child.offsetMin = new Vector2(step, -height);
                        child.offsetMax = new Vector2(width + step, 0);
                        child.ForceUpdateRectTransforms();
                        step += childRect.width;
                        break;
                    case RectTransform.Axis.Vertical:
                        child.anchorMin = new Vector2(0, 1);
                        child.anchorMax = new Vector2(0, 1);
                        child.offsetMin = new Vector2(0, -height - step);
                        child.offsetMax = new Vector2(width, -step);
                        child.ForceUpdateRectTransforms();
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
