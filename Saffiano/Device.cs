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

        public abstract void BeginScene(Camera camera);

        public abstract void EndScene();

        public abstract void Clear(Color backgroundColor);

        public virtual void SetViewport(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public abstract void Dispose();

        public abstract void Draw(Command command);

        public abstract void UpdateTexture(Texture texture, uint x, uint y, uint blockWidth, uint blockHeight, Color[] pixels);

        public abstract void Start();

        public abstract void End();
    }
}
