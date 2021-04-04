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

        private bool dirty = true;

        private Vector3 _localPosition = Vector3.zero;

        public override Vector3 localPosition
        {
            get
            {
                if (dirty)
                {
                    ForceUpdateRectTransforms();
                }
                return _localPosition;
            }
            set
            {
                throw new NotImplementedException();
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
                if (data.anchorMax == value)
                {
                    return;
                }
                data.anchorMax = value;
                dirty = true;
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
                if (data.anchorMin == value)
                {
                    return;
                }
                data.anchorMin = value;
                dirty = true;
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
                if (data.offsetMax == value)
                {
                    return;
                }
                data.offsetMax = value;
                dirty = true;
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
                if (data.offsetMin == value)
                {
                    return;
                }
                data.offsetMin = value;
                dirty = true;
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
                if (data.pivot == value)
                {
                    return;
                }
                data.pivot = value;
                dirty = true;
            }
        }

        private Rect _rect = Rect.zero;

        public Rect rect
        {
            get
            {
                if (dirty)
                {
                    ForceUpdateRectTransforms();
                }
                return _rect;
            }
        }

        protected override void OnParentChanged(Transform old, Transform current)
        {
            dirty = true;
            base.OnParentChanged(old, current);
        }

        public void ForceUpdateRectTransforms()
        {
            if (GetComponent<Canvas>() == null && this.parent as RectTransform == null)
            {
                return;
            }

            var parent = this.parent as RectTransform;

            // current transform rect in parent space
            Rect rect = new Rect();
            if (GetComponent<Canvas>() != null)
            {
                rect.size = offsetMax - offsetMin;
            }
            else
            {
                rect = parent.rect;
                var lb = rect.size * anchorMin + rect.position + offsetMin;
                var rt = rect.size * anchorMax + rect.position + offsetMax;
                rect = new Rect(lb, rt - lb);
            }

            // pivot in parent space
            Vector2 pivot = rect.position + this.pivot * rect.size;
            
            _localPosition = new Vector3(pivot, 0);

            // convert rect to current space
            rect.position -= pivot;

            if (this._rect != rect)
            {
                this._rect = rect;
                foreach (RectTransform child in this)
                {
                    child.dirty = true;
                    child.SendMessage("OnTransformParentChanged");
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
        }

        public void SetSizeWithCurrentAnchors(RectTransform.Axis axis, float size)
        {
            float low, high;
            ForceUpdateRectTransforms();
            switch (axis)
            {
                case Axis.Horizontal:
                    low = -this.pivot.x * size;
                    high = (1 - this.pivot.x) * size;
                    data.offsetMin = new Vector2(low - (this._rect.left - data.offsetMin.x), data.offsetMin.y);
                    data.offsetMax = new Vector2(high - (this._rect.right - data.offsetMax.x), data.offsetMax.y);
                    break;
                case Axis.Vertical:
                    low = -this.pivot.y * size;
                    high = (1 - this.pivot.y) * size;
                    data.offsetMin = new Vector2(data.offsetMin.x, low - (this._rect.bottom - data.offsetMin.y));
                    data.offsetMax = new Vector2(data.offsetMax.x, high - (this._rect.top - data.offsetMax.y));
                    break;
            }
            dirty = true;
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
