using System;

namespace Saffiano
{
    internal class Viewport
    {
        public uint x
        {
            get;
            set;
        }

        public uint y
        {
            get;
            set;
        }

        public uint width
        {
            get;
            set;
        }

        public uint height
        {
            get;
            set;
        }

        public float minZ
        {
            get;
            set;
        }

        public float maxZ
        {
            get;
            set;
        }
    }

    internal enum TransformStateType
    {
        View = 2,
        Projection = 3,
    }

    internal abstract class Device : IDisposable
    {
        public Viewport viewport
        {
            get;
            private set;
        }

        public abstract void BeginScene();

        public abstract void EndScene();

        public abstract void Clear();

        public virtual void SetViewport(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public abstract void SetTransform(TransformStateType state, Matrix4x4 matrix);

        public abstract void Dispose();

        public abstract void RegisterMesh(Mesh mesh);

        public abstract void UnregisterMesh(Mesh mesh);

        public abstract void DrawMesh(Mesh mesh);

        public abstract CoordinateSystems coordinateSystem
        {
            get;
        }
    }
}
