using OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Saffiano
{
    internal class OpenGLDevice : Device
    {
        private DeviceContext deviceContext;
        private IntPtr glContext;

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
        }

        public override void EndScene()
        {
            SwapBuffers();
            Gl.Flush();
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
            Gl.LoadMatrix(matrix.inverse.ToArray());
            
        }

        #region OpenGL vertex array buffer

        Dictionary<Guid, uint> vertexCache = new Dictionary<Guid, uint>();

        public override void RegisterMesh(Mesh mesh)
        {
            if (vertexCache.ContainsKey(mesh.id))
            {
                throw new Exception();
            }

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vbo = Gl.GenBuffer();

            if (mesh.vertices == null)
            {
                throw new Exception();
            }
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.vertices.Length), mesh.vertices, BufferUsage.StaticDraw);
            Gl.EnableClientState(EnableCap.VertexArray);
            Gl.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            if (mesh.normals != null)
            {
                uint nbo = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, nbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.normals.Length), mesh.normals, BufferUsage.StaticDraw);
                Gl.EnableClientState(EnableCap.NormalArray);
                Gl.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);
            }

            if (mesh.uv != null)
            {
                uint uvbo = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, uvbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector2)) * mesh.uv.Length), mesh.uv, BufferUsage.StaticDraw);
                Gl.EnableClientState(EnableCap.TextureCoordArray);
                Gl.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);
            }

            if (mesh.indices == null)
            {
                throw new Exception();
            }
            uint ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * mesh.indices.Length), mesh.indices, BufferUsage.StaticDraw);

            vertexCache.Add(mesh.id, vao);
        }

        public override void UnregisterMesh(Mesh mesh)
        {
            if (!vertexCache.ContainsKey(mesh.id))
            {
                throw new Exception();
            }

            Gl.DeleteVertexArrays(new uint[] { vertexCache[mesh.id] });
            vertexCache.Remove(mesh.id);
        }

        #endregion

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

        public override void DrawMesh(Mesh mesh)
        {
            if (!vertexCache.ContainsKey(mesh.id))
            {
                throw new Exception();
            }
            Gl.Enable(EnableCap.Lighting);
            Gl.Enable(EnableCap.DepthTest);
            Gl.BindVertexArray(this.vertexCache[mesh.id]);
            OpenGL.PrimitiveType primitiveType = OpenGLDevice.ConvertPrimitiveTypeToOpenGL(mesh.primitiveType);
            Gl.DrawElements(OpenGL.PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindTexture(TextureTarget.Texture2d, 0);
        }
    }
}
