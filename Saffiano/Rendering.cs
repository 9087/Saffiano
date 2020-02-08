using System;
using System.Collections.Generic;
using System.Linq;

namespace Saffiano
{
    internal sealed class Rendering
    {
        private static Device device;

        private static Stack<Matrix4x4> projections = new Stack<Matrix4x4>();

        internal static void PushProjection(Matrix4x4 matrix)
        {
            projections.Push(matrix);
            device.SetTransform(TransformStateType.Projection, projections.Peek());
        }

        internal static void PopProjection()
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

        internal static void SetTransform(TransformStateType transformStateType, Transform transform)
        {
            device.SetTransform(transformStateType, transform.ToRenderingMatrix(device.coordinateSystem));
        }

        private static void Traverse(Transform transform)
        {
            SetTransform(TransformStateType.View, transform);
            transform.GetComponent<LODGroup>()?.Update(Camera.main);
            transform.GetComponent<MeshRenderer>()?.Render();
            foreach (Transform child in transform)
            {
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
            Canvas.Render(Camera.main);
            device.EndScene();
            return true;
        }

        public static void Draw(Command command)
        {
            device.Draw(command);
        }
    }
}
