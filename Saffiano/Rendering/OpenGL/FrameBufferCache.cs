using OpenGL;

namespace Saffiano.Rendering.OpenGL
{
    internal class FrameBufferData
    {
        public uint fbo { get; internal set; }
    }

    internal class FrameBuffer
    {
        public virtual bool OnRegister(OpenGLDevice device, uint fbo)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool OnUnregister(OpenGLDevice device)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class RenderTextureFrameBuffer : FrameBuffer
    {
        public RenderTexture renderTexture { get; private set; } = null;

        protected bool depth = false;

        protected uint depthRenderbuffer = 0;

        public RenderTextureFrameBuffer(RenderTexture renderTexture, bool depth = false)
        {
            this.renderTexture = renderTexture;
            this.depth = depth;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RenderTextureFrameBuffer))
            {
                return false;
            }
            return this.renderTexture == (obj as RenderTextureFrameBuffer).renderTexture;
        }

        public override int GetHashCode()
        {
            return this.renderTexture.GetHashCode();
        }

        public override bool OnRegister(OpenGLDevice device, uint fbo)
        {
            uint textureID = device.textureCache.TryRegister(this.renderTexture);
            bool multisampling = this.renderTexture.multisampling;
            Gl.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                multisampling ? TextureTarget.Texture2dMultisample : TextureTarget.Texture2d,
                textureID,
                0);
            if (this.depth)
            {
                depthRenderbuffer = Gl.GenRenderbuffer();
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                if (multisampling)
                {
                    Gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, (int)QualitySettings.antiAliasing,
                        InternalFormat.DepthComponent32, (int)renderTexture.width, (int)renderTexture.height);
                }
                else
                {
                    Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                        InternalFormat.DepthComponent32, (int)renderTexture.width, (int)renderTexture.height);
                }
                Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            }
            return true;
        }

        public override bool OnUnregister(OpenGLDevice device)
        {
            if (this.depth)
            {
                Gl.DeleteRenderbuffers(new uint[] { depthRenderbuffer });
                depthRenderbuffer = 0;
            }
            return true;
        }
    }

    internal class FrameBufferCache : Cache<FrameBuffer, FrameBufferData>
    {
        protected override FrameBufferData OnRegister(FrameBuffer key)
        {
            uint fbo = Gl.CreateFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            key.OnRegister(device as OpenGLDevice, fbo);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return new FrameBufferData() { fbo = fbo };
        }

        protected override void OnUnregister(FrameBuffer key)
        {
            key.OnUnregister(device as OpenGLDevice);
            Gl.DeleteFramebuffers(this[key].fbo);
        }
    }
}
