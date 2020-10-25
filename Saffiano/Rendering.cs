using System;
using System.Collections.Generic;
using System.Linq;

namespace Saffiano
{
    internal sealed class Rendering
    {
        public static Device device { get; private set; }

        private static Stack<Matrix4x4> projections = new Stack<Matrix4x4>();

        public static Matrix4x4 projection => projections.Peek();

        internal static void PushProjection(Matrix4x4 matrix)
        {
            projections.Push(matrix);
        }

        internal static void PopProjection()
        {
            projections.Pop();
            if (projections.Count > 0)
            {
                var peek = projections.Peek();
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

        private static void Traverse(Transform transform)
        {
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
            Traverse(Transform.scene);
            PopProjection();
            Canvas.Render(Camera.main);
            device.EndScene();
            return true;
        }

        public static void Draw(Command command)
        {
            if (command != null && command.mainTexture != null && command.mainTexture.atlas != null)
            {
                throw new NotImplementedException();
            }
            device.Draw(command);
        }

        public static void UpdateTexture(Texture texture, uint x, uint y, uint blockWidth, uint blockHeight, Color[] pixels)
        {
            device.UpdateTexture(texture, x, y, blockWidth, blockHeight, pixels);
        }
    }
}
