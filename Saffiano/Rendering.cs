﻿namespace Saffiano
{
    internal sealed class Rendering
    {
        private static Device device;

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

        private static void Traverse(Transform root)
        {
            root.GetComponent<Renderer>()?.Render();
            foreach (Transform child in root)
            {
                Traverse(child);
            }
        }

        private static bool Update()
        {
            device.BeginScene();
            device.Clear();
            device.SetTransform(TransformStateType.Projection, Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix);
            device.SetTransform(TransformStateType.View, Matrix4x4.identity);
            Traverse(Transform.root);
            device.EndScene();
            return true;
        }

        public static void RegisterMesh(Mesh mesh)
        {
            device.RegisterMesh(mesh);
        }

        public static void UnregisterMesh(Mesh mesh)
        {
            device.UnregisterMesh(mesh);
        }

        public static void DrawMesh(Mesh mesh)
        {
            device.DrawMesh(mesh);
        }
    }
}