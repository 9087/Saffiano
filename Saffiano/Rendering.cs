using System;
using System.Collections.Generic;

namespace Saffiano
{
    internal sealed class Rendering
    {
        private static Device device;

        private static Stack<Matrix4x4> projections = new Stack<Matrix4x4>();

        private static void PushProjection(Matrix4x4 matrix)
        {
            projections.Push(matrix);
            device.SetTransform(TransformStateType.Projection, projections.Peek());
        }

        private static void PopProjection()
        {
            projections.Pop();
            if (projections.Count > 0)
            {
                var peek = projections.Peek();
                device.SetTransform(TransformStateType.Projection, peek);
            }
        }

        public static Viewport viewport
        {
            get
            {
                return device.viewport;
            }
        }

        private static void Initialize()
        {
            device = new OpenGLDevice(Window.window);
            Window.Resized += OnWindowResized;
            Vector2 size = Window.GetSize();
            device.SetViewport(new Viewport() { width = (uint)size.x, height = (uint)size.y, });
        }

        private static void OnWindowResized(Vector2 size)
        {
            device.SetViewport(new Viewport() { width = (uint)size.x, height = (uint)size.y, });
        }

        private static void Uninitialize()
        {
            Window.Resized -= OnWindowResized;
            device.Dispose();
            device = null;
        }

        private static void TraverseScreenSpaceCanvas(Transform transform)
        {
            var size = Window.GetSize();
            PushProjection(Matrix4x4.Scaled(new Vector3(2.0f / size.x, 2.0f / size.y, 0)));
            Traverse(transform);
            PopProjection();
        }

        private static void Traverse(Transform transform)
        {
            device.SetTransform(TransformStateType.View, transform.ToRenderingMatrix(device.coordinateSystem));
            transform.GetComponent<LODGroup>()?.Update(Camera.main);
            transform.GetComponent<Renderer>()?.Render();
            foreach (Transform child in transform)
            {
                var canvas = child.GetComponent<Canvas>();
                switch (canvas?.renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        TraverseScreenSpaceCanvas(child);
                        continue;
                    case RenderMode.ScreenSpaceOverlay:
                        Canvas.overlayCanvases.Add(canvas);
                        continue;
                }
                Traverse(child);
            }
        }

        private static bool Update()
        {
            device.BeginScene();
            device.Clear();
            PushProjection(Camera.main.projectionMatrix * Camera.main.transform.GenerateWorldToLocalMatrix(device.coordinateSystem));
            device.SetTransform(TransformStateType.View, Matrix4x4.identity);
            Traverse(Transform.root);
            PopProjection();
            foreach (var canvas in Canvas.overlayCanvases)
            {
                TraverseScreenSpaceCanvas(canvas.transform);
            }
            Canvas.overlayCanvases.Clear();
            device.EndScene();
            return true;
        }

        public static void Draw(Command command)
        {
            device.Draw(command);
        }
    }
}
