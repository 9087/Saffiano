using System;

namespace Saffiano
{
    public sealed class RectTransform : Transform
    {
        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1,
        }

        public enum Edge
        {
            Left = 0,
            Right = 1,
            Top = 2,
            Bottom = 3,
        }

        private struct InternalData
        {
            public Vector2 anchorMax;
            public Vector2 anchorMin;
            public Vector2 offsetMax;
            public Vector2 offsetMin;
            public Vector2 pivot;
        }

        private InternalData data = new InternalData()
        {
            anchorMax = new Vector2(0.5f, 0.5f),
            anchorMin = new Vector2(0.5f, 0.5f),
            offsetMax = new Vector2(128, 128),
            offsetMin = new Vector2(-128, -128),
            pivot = new Vector2(0.5f, 0.5f),
        };

        public override Vector3 localPosition
        {
            get
            {
                var contentRect = GetContentRectToParent();
                return new Vector3(contentRect.left + contentRect.width * pivot.x, contentRect.bottom + contentRect.height * pivot.y, 0);
            }
            set
            {
                var contentRect = GetContentRectToParent();
                pivot = new Vector2((value.x - contentRect.left) / contentRect.width, (value.y - contentRect.bottom) / contentRect.height);
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Vector2 anchorMax
        {
            get
            {
                return data.anchorMax;
            }
            set
            {
                data.anchorMax = value;
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Vector2 anchorMin
        {
            get
            {
                return data.anchorMin;
            }
            set
            {
                data.anchorMin = value;
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Vector2 offsetMax
        {
            get
            {
                return data.offsetMax;
            }
            set
            {
                data.offsetMax = value;
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Vector2 offsetMin
        {
            get
            {
                return data.offsetMin;
            }
            set
            {
                data.offsetMin = value;
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Vector2 pivot
        {
            get
            {
                return data.pivot;
            }
            set
            {
                data.pivot = value;
                ForceUpdateRectTransforms();
                SendMessage("OnTransformParentChanged");
            }
        }

        public Rect rect { get; private set; } = Rect.zero;

        protected override void OnParentChanged(Transform old, Transform current)
        {
            base.OnParentChanged(old, current);
            ForceUpdateRectTransforms();
        }

        internal void OnParentResized(Vector2 size)
        {
            ForceUpdateRectTransforms();
            SendMessage("OnTransformParentChanged");
        }

        internal Rect GetAnchorRectToParent()  // anchor rect to parent
        {
            RectTransform parent = this.parent as RectTransform;
            Vector2 parentSize = Vector2.zero;
            Canvas canvas = GetComponent<Canvas>();
            Debug.Assert(canvas != null || parent != null);
            if (canvas == null)
            {
                parentSize = parent.rect.size;
            }
            else
            {
                parentSize = Window.GetSize();
            }
            var anchorSize = anchorMax - anchorMin;
            Rect anchorRect = new Rect()
            {
                x = (anchorMin.x - 0.5f) * parentSize.x,
                y = (anchorMin.y - 0.5f) * parentSize.y,
                width = anchorSize.x * parentSize.x,
                height = anchorSize.y * parentSize.y,
            };
            return anchorRect;
        }

        internal Rect GetContentRectToParent()
        {
            var anchorRect = this.GetAnchorRectToParent();
            Rect contentRect = Rect.zero;
            contentRect.left = anchorRect.left + offsetMin.x;
            contentRect.right = anchorRect.right + offsetMax.x;
            contentRect.bottom = anchorRect.bottom + offsetMin.y;
            contentRect.top = anchorRect.top + offsetMax.y;
            return contentRect;
        }

        public void ForceUpdateRectTransforms()
        {
            if (GetComponent<Canvas>() == null && this.parent as RectTransform == null)
            {
                return;
            }
            Rect contentRect = this.GetContentRectToParent();
            Rect rect = Rect.zero;  // content rect to pivot
            rect.left = -pivot.x * contentRect.width;
            rect.right = (1 - pivot.x) * contentRect.width;
            rect.bottom = -pivot.y * contentRect.height;
            rect.top = (1 - pivot.y) * contentRect.height;
            var lastSize = this.rect.size;
            var currentSize = rect.size;
            this.rect = rect;
            if (lastSize != currentSize)
            {
                foreach (RectTransform child in this)
                {
                    child.OnParentResized(currentSize);
                }
            }
        }

        public override Matrix4x4 localToWorldMatrix
        {
            get
            {
                var canvas = GetComponent<Canvas>();
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                {
                    return base.localToWorldMatrix;
                }
                else
                {
                    return canvas.targetCamera.transform.localToWorldMatrix;
                }
            }
        }

        public void SetInsetAndSizeFromParentEdge(Edge edge, float inset, float size)
        {
            switch (edge)
            {
                case Edge.Bottom:
                    anchorMin = new Vector2(anchorMin.x, 0);
                    anchorMax = new Vector2(anchorMax.x, 0);
                    offsetMin = new Vector2(offsetMin.x, inset);
                    offsetMax = new Vector2(offsetMax.x, inset + size);
                    pivot = new Vector2(pivot.x, 0);
                    break;
                case Edge.Left:
                    anchorMin = new Vector2(0, anchorMin.y);
                    anchorMax = new Vector2(0, anchorMax.y);
                    offsetMin = new Vector2(inset, offsetMin.y);
                    offsetMax = new Vector2(inset + size, offsetMin.y);
                    pivot = new Vector2(0, pivot.y);
                    break;
                case Edge.Right:
                    anchorMin = new Vector2(1, anchorMin.y);
                    anchorMax = new Vector2(1, anchorMax.y);
                    offsetMin = new Vector2(-inset - size, offsetMin.y);
                    offsetMax = new Vector2(-inset, offsetMin.y);
                    pivot = new Vector2(1, pivot.y);
                    break;
                case Edge.Top:
                    anchorMin = new Vector2(anchorMin.x, 1);
                    anchorMax = new Vector2(anchorMax.x, 1);
                    offsetMin = new Vector2(offsetMin.x, -inset - size);
                    offsetMax = new Vector2(offsetMax.x, -inset);
                    pivot = new Vector2(pivot.x, 1);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ForceUpdateRectTransforms();
            SendMessage("OnTransformParentChanged");
        }

        public void SetSizeWithCurrentAnchors(RectTransform.Axis axis, float size)
        {
            var half = size * (axis == Axis.Horizontal ? this.pivot.x : this.pivot.y);
            var centerAxis = axis == Axis.Horizontal ? this.localPosition.x : this.localPosition.y;
            var lowerAxis = centerAxis - half;
            var upperAxis = centerAxis + half;
            var anchorRect = GetAnchorRectToParent();
            var anchorMinAxis = axis == Axis.Horizontal ? anchorRect.left : anchorRect.bottom;
            var anchorMaxAxis = axis == Axis.Horizontal ? anchorRect.right : anchorRect.top;
            var offsetMinAxis = lowerAxis - anchorMinAxis;
            var offsetMaxAxis = upperAxis - anchorMaxAxis;
            switch (axis)
            {
                case Axis.Horizontal:
                    offsetMin = new Vector2(offsetMinAxis, offsetMin.y);
                    offsetMax = new Vector2(offsetMaxAxis, offsetMax.y);
                    break;
                case Axis.Vertical:
                    offsetMin = new Vector2(offsetMin.x, offsetMinAxis);
                    offsetMax = new Vector2(offsetMax.x, offsetMaxAxis);
                    break;
            }
            ForceUpdateRectTransforms();
            SendMessage("OnTransformParentChanged");

        }

        protected override void OnChildAdded(Transform child)
        {
            base.OnChildAdded(child);
            Array.ForEach(GetComponents<Behaviour>(), (b) => { b.Invoke("OnRectTransformDimensionsChange"); });
        }

        protected override void OnChildRemoved(Transform child)
        {
            base.OnChildRemoved(child);
            Array.ForEach(GetComponents<Behaviour>(), (b) => { b.Invoke("OnRectTransformDimensionsChange"); });
        }
    }
}
