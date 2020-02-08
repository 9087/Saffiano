using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saffiano
{
    public enum RenderMode
    {
        ScreenSpaceOverlay = 0,
        ScreenSpaceCamera = 1,
        WorldSpace = 2
    }

    public sealed class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; } = RenderMode.ScreenSpaceOverlay;

        internal Camera targetCamera => Camera.main;

        internal static List<Canvas> canvases = new List<Canvas>();

        private RectTransform rectTransform => this.transform as RectTransform;

        private static void Initialize()
        {
            Window.Resized += OnWindowResized;
        }

        private static void Uninitialize()
        {
            Window.Resized -= OnWindowResized;
        }

        private static void OnWindowResized(Vector2 size)
        {
            foreach (Canvas canvas in canvases)
            {
                canvas.rectTransform.OnParentResized(size);
            }
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            base.OnComponentAdded(gameObject);
            RectTransform rectTransform = this.rectTransform;
            switch (renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.offsetMin = new Vector2(0, 0);
                    rectTransform.offsetMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    throw new NotImplementedException();
            }
            rectTransform.ForceUpdateRectTransforms();
            canvases.Add(this);
        }

        internal override void OnComponentRemoved()
        {
            canvases.Remove(this);
            base.OnComponentRemoved();
        }

        internal static void Traverse(RectTransform rectTransform)
        {
            Rendering.SetTransform(TransformStateType.View, rectTransform);
            rectTransform.GetComponent<CanvasRenderer>()?.Render();
            foreach (RectTransform child in rectTransform)
            {
                Traverse(child);
            }
        }

        internal static void Render(Camera camera)
        {
            foreach (var canvas in Canvas.canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }
                Canvas.Render(canvas);
            }
            foreach (var canvas in Canvas.canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }
                Canvas.Render(canvas);
            }
        }

        private static void Render(Canvas canvas)
        {
            var size = Window.GetSize();
            Rendering.PushProjection(Matrix4x4.Scaled(new Vector3(2.0f / size.x, 2.0f / size.y, 0)));
            Traverse(canvas.rectTransform);
            Rendering.PopProjection();
        }
    }
}
