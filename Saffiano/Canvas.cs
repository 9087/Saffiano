﻿using System;
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
        }

        internal override void OnComponentRemoved()
        {
            base.OnComponentRemoved();
        }

        internal static void Traverse(Camera camera, RectTransform rectTransform)
        {
            if (!camera.IsCulled(rectTransform.gameObject))
            {
                rectTransform.GetComponent<CanvasRenderer>()?.Render();
            }
            foreach (RectTransform child in rectTransform)
            {
                Traverse(camera, child);
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
                Canvas.Render(camera, canvas);
            }
            foreach (var canvas in Canvas.canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }
                Canvas.Render(camera, canvas);
            }
        }

        private static void Render(Camera camera, Canvas canvas)
        {
            if (!canvas.gameObject.activeInHierarchy)
            {
                return;
            }
            var size = Window.GetSize();
            Rendering.PushProjection(Matrix4x4.Scaled(new Vector3(1.0f / (int)(size.x / 2), 1.0f / (int)(size.y / 2), 0)));
            Traverse(camera, canvas.rectTransform);
            Rendering.PopProjection();
        }

        void OnEnable()
        {
            canvases.Add(this);
        }

        void OnDisable()
        {
            canvases.Remove(this);
        }
    }
}
