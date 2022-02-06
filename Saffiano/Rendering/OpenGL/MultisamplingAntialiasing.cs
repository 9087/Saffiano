using OpenGL;
using static Saffiano.Resources.Default.Mesh;

namespace Saffiano.Rendering.OpenGL
{
    internal class MultisamplingAntialiasingMaterial : ScriptableMaterial
    {
        public override ZTest zTest { get; set; } = ZTest.Always;

        public virtual void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.TexCoord)] Vector2 a_texcoord,
            out Vector4 gl_Position,
            out Vector2 v_texcoord
        )
        {
            v_texcoord = a_texcoord;
            gl_Position = new Vector4(a_position.x, a_position.z, 0, 1.0f);
        }

        public virtual void FragmentShader(
            Vector2 v_texcoord,
            out Color f_color
        )
        {
            f_color = mainTexture.Sample(v_texcoord);
        }
    }

    internal class MultisamplingAntialiasing
    {
        RenderTexture textureColorBufferMultiSampled = null;
        RenderTexture screenTexture = null;
        Mesh mesh;
        Material material;

        uint textureColorBufferMultiSampledFrameBufferID;
        uint screenTextureFrameBufferID;

        public MultisamplingAntialiasing()
        {
            mesh = new Plane(Vector2.one * 2);
            material = new MultisamplingAntialiasingMaterial();
        }

        public void BeginScene(OpenGLDevice device)
        {
            if (QualitySettings.antiAliasing != 1)
            {
                if (textureColorBufferMultiSampled == null ||
                    textureColorBufferMultiSampled.width != device.viewport.width ||
                    textureColorBufferMultiSampled.height != device.viewport.height)
                {
                    textureColorBufferMultiSampled = new RenderTexture(device.viewport.width, device.viewport.height, multisampling: true);
                }
                if (screenTexture == null ||
                    screenTexture.width != device.viewport.width ||
                    screenTexture.height != device.viewport.height)
                {
                    screenTexture = new RenderTexture(device.viewport.width, device.viewport.height);
                }
                textureColorBufferMultiSampledFrameBufferID = device.frameBufferCache.TryRegister(new RenderTextureFrameBuffer(textureColorBufferMultiSampled, depth: true)).fbo;
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, textureColorBufferMultiSampledFrameBufferID);
            }
            else
            {
                textureColorBufferMultiSampled = null;
                screenTexture = null;
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }

        public void EndScene(OpenGLDevice device)
        {
            if (QualitySettings.antiAliasing == 1)
            {
                return;
            }
            screenTextureFrameBufferID = device.frameBufferCache.TryRegister(new RenderTextureFrameBuffer(screenTexture)).fbo;
            Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, textureColorBufferMultiSampledFrameBufferID);
            Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, screenTextureFrameBufferID);
            Gl.BlitFramebuffer(
                0, 0, (int)textureColorBufferMultiSampled.width, (int)textureColorBufferMultiSampled.height,
                0, 0, (int)screenTexture.width, (int)screenTexture.height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            device.Draw(new Command()
            {
                projection = Matrix4x4.identity,
                transform = Matrix4x4.identity,
                mesh = mesh,
                mainTexture = screenTexture,
                lighting = false,
                material = material,
            });
        }
    }
}
