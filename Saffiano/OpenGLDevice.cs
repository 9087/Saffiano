using OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Saffiano
{
    internal class VertexData
    {
        public uint vao { get; internal set; }

        public uint vbo { get; internal set; }

        public uint nbo { get; internal set; }

        public uint uvbo { get; internal set; }

        public uint ebo { get; internal set; }
    }

    internal class VertexCache : Cache<Mesh, VertexData>
    {
        protected override VertexData OnRegister(Mesh mesh)
        {
            VertexData vertexData = new VertexData();

            vertexData.vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vertexData.vao);

            vertexData.vbo = Gl.GenBuffer();
            if (mesh.vertices == null)
            {
                throw new Exception();
            }
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.vertices.Length), mesh.vertices, BufferUsage.StaticDraw);
            Gl.EnableClientState(EnableCap.VertexArray);
            Gl.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            vertexData.nbo = Gl.GenBuffer();
            if (mesh.normals != null)
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.nbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.normals.Length), mesh.normals, BufferUsage.StaticDraw);
                Gl.EnableClientState(EnableCap.NormalArray);
                Gl.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);
            }

            vertexData.uvbo = Gl.GenBuffer();
            if (mesh.uv != null)
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.uvbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector2)) * mesh.uv.Length), mesh.uv, BufferUsage.StaticDraw);
                Gl.EnableClientState(EnableCap.TextureCoordArray);
                Gl.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);
            }

            vertexData.ebo = Gl.GenBuffer();
            if (mesh.indices == null)
            {
                throw new Exception();
            }
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, vertexData.ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * mesh.indices.Length), mesh.indices, BufferUsage.StaticDraw);

            return vertexData;
        }

        protected override void OnUnregister(Mesh mesh)
        {
            Gl.DeleteBuffers(this[mesh].vbo, this[mesh].nbo, this[mesh].uvbo, this[mesh].ebo);
            Gl.DeleteVertexArrays(this[mesh].vao);
        }
    }

    internal class TextureCache : Cache<Texture, uint>
    {
        protected override uint OnRegister(Texture texture)
        {
            uint textureID = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureID);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)texture.width, (int)texture.height, 0, PixelFormat.Rgba, PixelType.Float, texture.GetPixels());
            Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, new int[] { Gl.NEAREST });
            Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, new int[] { Gl.NEAREST });
            Gl.GenerateMipmap(TextureTarget.Texture2d);
            texture.OnRegister();
            return textureID;
        }

        protected override void OnUnregister(Texture texture)
        {
            texture.OnUnregister();
            Gl.DeleteTextures(this[texture]);
        }
    }

    internal class OpenGLDevice : Device
    {
        private DeviceContext deviceContext;
        private IntPtr glContext;

        private HashSet<Mesh> requestedMeshes = new HashSet<Mesh>();
        VertexCache vertexCache = new VertexCache();

        private HashSet<Texture> requestedTextures = new HashSet<Texture>();
        TextureCache textureCache = new TextureCache();

        public override CoordinateSystems coordinateSystem => CoordinateSystems.RightHand;

        static OpenGLDevice()
        {
            Gl.Initialize();
        }

        public OpenGLDevice(Win32Window window)
        {
            this.deviceContext = DeviceContext.Create(IntPtr.Zero, window.handle);
            this.deviceContext.IncRef();

            Debug.LogFormat("OpenGL API: {0}", this.deviceContext.CurrentAPI);

            List<DevicePixelFormat> pixelFormats = this.deviceContext.PixelsFormats.Choose(
                new DevicePixelFormat
                {
                    RgbaUnsigned = true,
                    RenderWindow = true,
                    ColorBits = 32,
                    DepthBits = 24,
                    StencilBits = 8,
                    MultisampleBits = 0,
                    DoubleBuffer = false,
                }
            );
            this.deviceContext.SetPixelFormat(pixelFormats[0]);

            if (Gl.PlatformExtensions.SwapControl)
            {
                int swapInterval = 1;

                // Mask value in case it is not supported
                if (!Gl.PlatformExtensions.SwapControlTear && swapInterval == -1)
                    swapInterval = 1;

                this.deviceContext.SwapInterval(swapInterval);
            }

            if (this.glContext != IntPtr.Zero)
                throw new InvalidOperationException("context already created");


            if ((this.glContext = this.deviceContext.CreateContext(IntPtr.Zero)) == IntPtr.Zero)
                throw new InvalidOperationException("unable to create render context");

            this.MakeCurrent();

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            Gl.DepthFunc(DepthFunction.Lequal);

            Gl.Light(LightName.Light0, LightParameter.Position, new float[] { 1.0f, 1.0f, 1.0f, 0.0f });
            Gl.Enable(EnableCap.Light0);

            Gl.Enable(EnableCap.Texture2d);
            Gl.Enable(EnableCap.TextureCoordArray);
            Gl.Enable(EnableCap.Blend);
            Gl.ShadeModel(ShadingModel.Smooth);
            Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        }

        public override void Dispose()
        {
            vertexCache.UnregisterAll();

            if (this.glContext == IntPtr.Zero)
            {
                return;
            }

            this.deviceContext.DeleteContext(this.glContext);
            this.glContext = IntPtr.Zero;

            if (this.deviceContext != null)
            {
                this.deviceContext.Dispose();
                this.deviceContext = null;
            }
        }

        private void MakeCurrent()
        {
            if (this.deviceContext.MakeCurrent(this.glContext) == false)
                throw new InvalidOperationException("unable to make context current");
        }

        private void SwapBuffers()
        {
            this.deviceContext.SwapBuffers();
        }

        public override void Clear()
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void BeginScene()
        {
            MakeCurrent();
            requestedMeshes.Clear();
            requestedTextures.Clear();
        }

        public override void EndScene()
        {
            SwapBuffers();
            Gl.Flush();
            vertexCache.Keep(requestedMeshes);
        }

        public override void SetViewport(Viewport viewport)
        {
            base.SetViewport(viewport);
            Gl.Viewport((int)viewport.x, (int)viewport.y, (int)viewport.width, (int)viewport.height);
        }

        public override void SetTransform(TransformStateType state, Matrix4x4 matrix)
        {
            MakeCurrent();
            switch(state)
            {
                case TransformStateType.Projection:
                    Gl.MatrixMode(MatrixMode.Projection);
                    break;
                case TransformStateType.View:
                    Gl.MatrixMode(MatrixMode.Modelview);
                    break;
                default:
                    throw new Exception();
            }
            Gl.LoadIdentity();
            Gl.LoadMatrix(matrix.transpose.ToArray());
            
        }

        private static OpenGL.PrimitiveType ConvertPrimitiveTypeToOpenGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Triangles:
                    return OpenGL.PrimitiveType.Triangles;
                case PrimitiveType.Quads:
                    return OpenGL.PrimitiveType.Quads;
                default:
                    throw new NotImplementedException();
            }
        }

        private void BindVertex(Mesh mesh)
        {
            Gl.BindVertexArray(vertexCache.TryRegister(mesh).vao);
            requestedMeshes.Add(mesh);
        }

        private void BindTexture(Texture texture)
        {
            if (texture == null)
            {
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                return;
            }
            Gl.BindTexture(TextureTarget.Texture2d, textureCache.TryRegister(texture));
            requestedTextures.Add(texture);
        }

        private void Enable(EnableCap cap, bool value)
        {
            if (value)
            {
                Gl.Enable(cap);
            }
            else
            {
                Gl.Disable(cap);
            }
        }

        public override void Draw(Command command)
        {
            var mesh = command.mesh;
            BindVertex(mesh);
            BindTexture(command.texture);
            Enable(EnableCap.Lighting, command.lighting);
            Enable(EnableCap.DepthTest, command.depthTest);
            OpenGL.PrimitiveType primitiveType = ConvertPrimitiveTypeToOpenGL(mesh.primitiveType);
            if (command.blend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }
            Gl.DrawElements(primitiveType, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public override void UpdateTexture(Texture texture, uint x, uint y, uint blockWidth, uint blockHeight, Color[] pixels)
        {
            if (!textureCache.TryGetValue(texture, out uint textureID))
            {
                throw new Exception("An unregisted texture is requesting for update");
            }
            Gl.BindTexture(TextureTarget.Texture2d, textureID);
            Gl.TexSubImage2D(TextureTarget.Texture2d, 0, (int)x, (int)y, (int)blockWidth, (int)blockHeight, PixelFormat.Rgba, PixelType.Float, pixels);
        }
    }
}
