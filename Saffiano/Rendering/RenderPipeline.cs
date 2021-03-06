﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Saffiano.Rendering
{
    internal sealed class RenderPipeline
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
        }

        private static void OnWindowResized(Vector2 size)
        {
        }

        private static void Uninitialize()
        {
            Window.Resized -= OnWindowResized;
            device.Dispose();
            device = null;
        }

        private static void Traverse(Camera camera, Transform transform)
        {
            if (!camera.IsCulled(transform.gameObject))
            {
                transform.GetComponent<LODGroup>()?.Update(camera.transform.position);
                transform.GetComponent<MeshRenderer>()?.Render();
            }
            foreach (Transform child in transform)
            {
                Traverse(camera, child);
            }
        }

        private static bool Update()
        {
            device.Start();
            foreach (var camera in Camera.allCameras)
            {
                Vector2 size;
                if (camera.TargetTexture == null)
                {
                    size = Window.GetSize();
                }
                else
                {
                    size = new Vector2(camera.TargetTexture.width, camera.TargetTexture.height);
                }
                device.SetViewport(new Viewport() { width = (uint)size.x, height = (uint)size.y, });
                device.BeginScene(camera.TargetTexture);
                device.Clear(camera.backgroundColor);
                PushProjection(camera.projectionMatrix * camera.worldToCameraMatrix);
                Traverse(camera, Transform.scene);
                PopProjection();
                Canvas.Render(camera);
                device.EndScene();
            }
            device.End();
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
