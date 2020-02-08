namespace Saffiano
{
    public sealed class RectTransform : Transform
    {
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
            }
        }

        public Rect rect
        {
            get;
            private set;
        }

        public void ForceUpdateRectTransforms()
        {
            Vector2 windowSize = Window.GetSize();
            var anchorSize = anchorMax - anchorMin;
            Rect anchor = new Rect()
            {
                x = (anchorMin.x - 0.5f) * windowSize.x,
                y = (anchorMin.y - 0.5f) * windowSize.y,
                width = anchorSize.x * windowSize.x,
                height = anchorSize.y * windowSize.y,
            };
            Rect screenRect = Rect.zero;
            screenRect.left = anchor.left + offsetMin.x;
            screenRect.right = anchor.right + offsetMax.x;
            screenRect.bottom = anchor.bottom + offsetMin.y;
            screenRect.top = anchor.top + offsetMax.y;
            position = new Vector3(screenRect.left + screenRect.width * pivot.x, screenRect.bottom + screenRect.height * pivot.y, 0);
            Rect rect = Rect.zero;
            rect.left = -pivot.x * screenRect.width;
            rect.right = (1 - pivot.x) * screenRect.width;
            rect.bottom = -pivot.y * screenRect.height;
            rect.top = (1 - pivot.y) * screenRect.height;
            this.rect = rect;
        }

        internal override Matrix4x4 ToRenderingMatrix(CoordinateSystems coordinateSystem)
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return base.ToRenderingMatrix(coordinateSystem);
            }
            else
            {
                return canvas.targetCamera.transform.ToRenderingMatrix(coordinateSystem);
            }
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            base.OnComponentAdded(gameObject);
            Window.Resized += OnWindowResized;
        }

        private void OnWindowResized(Vector2 size)
        {
            ForceUpdateRectTransforms();
        }

        internal override void OnComponentRemoved()
        {
            Window.Resized -= OnWindowResized;
            base.OnComponentRemoved();
        }
    }
}
