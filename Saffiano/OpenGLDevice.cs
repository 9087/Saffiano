using OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saffiano
{
    internal class VertexData
    {
        public uint vao { get; internal set; }

        public uint vbo { get; internal set; }

        public uint nbo { get; internal set; }

        public uint uvbo { get; internal set; }

        public uint ebo { get; internal set; }

        public uint colorbo { get; internal set; }
    }

    internal enum GenericVertexAttributeLocation
    {
        Position = 0,
        Normal = 1,
        TexCoord = 2,
        Color = 3,
    }

    internal class VertexCache : Cache<Mesh, VertexData>
    {
        protected override VertexData OnRegister(Mesh mesh)
        {
            VertexData vertexData = new VertexData();

            vertexData.vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vertexData.vao);

            // vertices
            vertexData.vbo = Gl.GenBuffer();
            if (mesh.vertices != null)
            {
                Gl.EnableVertexAttribArray((uint)GenericVertexAttributeLocation.Position);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.vbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.vertices.Length), mesh.vertices, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)GenericVertexAttributeLocation.Position, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            }
            else
            {
                throw new Exception();
            }

            // normals
            vertexData.nbo = Gl.GenBuffer();
            if (mesh.normals != null)
            {
                Gl.EnableVertexAttribArray((uint)GenericVertexAttributeLocation.Normal);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.nbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector3)) * mesh.normals.Length), mesh.normals, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)GenericVertexAttributeLocation.Normal, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            }
            else
            {
                Gl.DisableVertexAttribArray((uint)GenericVertexAttributeLocation.Normal);
            }

            // uv
            vertexData.uvbo = Gl.GenBuffer();
            if (mesh.uv != null)
            {
                Gl.EnableVertexAttribArray((uint)GenericVertexAttributeLocation.TexCoord);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.uvbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Vector2)) * mesh.uv.Length), mesh.uv, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)GenericVertexAttributeLocation.TexCoord, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
            }
            else
            {
                Gl.DisableVertexAttribArray((uint)GenericVertexAttributeLocation.TexCoord);
            }

            // color
            vertexData.colorbo = Gl.GenBuffer();
            if (mesh.colors != null)
            {
                Gl.EnableVertexAttribArray((uint)GenericVertexAttributeLocation.Color);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexData.colorbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(Color)) * mesh.colors.Length), mesh.colors, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)GenericVertexAttributeLocation.Color, 4, VertexAttribType.Float, false, 0, IntPtr.Zero);
            }
            else
            {
                Gl.DisableVertexAttribArray((uint)GenericVertexAttributeLocation.Color);
            }

            vertexData.ebo = Gl.GenBuffer();
            if (mesh.indices == null)
            {
                throw new Exception();
            }
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, vertexData.ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * mesh.indices.Length), mesh.indices, BufferUsage.StaticDraw);

            Gl.BindVertexArray(0);

            return vertexData;
        }

        protected override void OnUnregister(Mesh mesh)
        {
            Gl.DeleteBuffers(this[mesh].vbo, this[mesh].nbo, this[mesh].uvbo, this[mesh].ebo, this[mesh].colorbo);
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

    internal class GPUProgramData
    {
        public uint program { get; internal set; }

        public uint vs { get; internal set; }

        public uint fs { get; internal set; }
    }

    internal class GPUProgramCache : Cache<GPUProgram, GPUProgramData>
    {
        private uint Compile(OpenGL.ShaderType shaderType, string source)
        {
            uint shader = Gl.CreateShader(shaderType);
            Gl.ShaderSource(shader, new string[] { source });
            Gl.CompileShader(shader);
            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
            Gl.GetShader(shader, ShaderParameterName.InfoLogLength, out int length);
            if (success == Gl.FALSE)
            {
                // compile error
                Gl.GetShader(shader, ShaderParameterName.ShaderSourceLength, out int sourceLength);
                StringBuilder logBuilder = new StringBuilder();
                Gl.GetShaderInfoLog(shader, length, out int _, logBuilder);
                Debug.LogWarning(string.Format("Shader compilation failed:\n{0}\n{1}", source, logBuilder.ToString()));
            }
            return shader;
        }

        protected override GPUProgramData OnRegister(GPUProgram key)
        {
            GPUProgramData shaderData = new GPUProgramData();
            shaderData.program = Gl.CreateProgram();
            shaderData.vs = Compile(OpenGL.ShaderType.VertexShader, key.vertexShaderSourceCode);
            shaderData.fs = Compile(OpenGL.ShaderType.FragmentShader, key.fragmentShaderSourceCode);
            Gl.AttachShader(shaderData.program, shaderData.vs);
            Gl.AttachShader(shaderData.program, shaderData.fs);
            Gl.LinkProgram(shaderData.program);
            return shaderData;
        }

        protected override void OnUnregister(GPUProgram key)
        {
            throw new NotImplementedException();
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

        internal GPUProgramCache shaderCache = new GPUProgramCache();

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
            Gl.ClearColor(0.192157f, 0.301961f, 0.474510f, 1.0f);
            Gl.DepthFunc(DepthFunction.Lequal);

            Gl.Light(LightName.Light0, LightParameter.Position, new float[] { 1.0f, 1.0f, 1.0f, 0.0f });
            Gl.Enable(EnableCap.Light0);

            Gl.Enable(EnableCap.Texture2d);
            Gl.Enable(EnableCap.TextureCoordArray);
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

        private bool BindTexture(int index, Texture texture)
        {
            if (texture == null)
            {
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                return false;
            }
            Gl.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + index));
            Gl.BindTexture(TextureTarget.Texture2d, textureCache.TryRegister(texture));
            requestedTextures.Add(texture);
            return true;
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

        private void SetProgramUniform(uint program, Uniform uniform, object value, object extra)
        {
            int location = Gl.GetUniformLocation(program, uniform.name);
            Type t = uniform.propertyInfo.PropertyType;
            switch (value)
            {
                case Matrix4x4 mat:
                    Gl.UniformMatrix4(location, true, mat.ToArray());
                    break;
                case Texture texture:
                    Gl.Uniform1(location, (int)extra);
                    break;
                case Vector4 vec4:
                    Gl.Uniform4(location, vec4.x, vec4.y, vec4.z, vec4.w);
                    break;
                case Color color:
                    Gl.Uniform4(location, color.r, color.g, color.b, color.a);
                    break;
                case Vector3 vec3:
                    Gl.Uniform3(location, vec3.x, vec3.y, vec3.z);
                    break;
                case Vector2 vec2:
                    Gl.Uniform2(location, vec2.x, vec2.y);
                    break;
                case float f:
                    Gl.Uniform1(location, f);
                    break;
                case int i:
                    Gl.Uniform1(location, i);
                    break;
                default:
                    throw new NotImplementedException(string.Format("uniform type {0} is not implemented.", value.GetType()));
            }
        }

        public override void Draw(Command command)
        {
            var mesh = command.mesh;
            BindVertex(mesh);
            Enable(EnableCap.Lighting, command.lighting);
            Enable(EnableCap.DepthTest, command.depthTest);
            if (command.blend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }

            // apply shader
            var material = command.material;
            var shader = material.shader;
            uint program = shaderCache.TryRegister(shader).program;
            Gl.UseProgram(program);
            int textureIndex = 0;
            foreach (var uniform in material.uniforms)
            {
                object value;
                object extra = null;
                switch (uniform.name)
                {
                    case "mvp":
                        value = command.projection * command.transform;
                        break;
                    case "mv":
                        value = command.transform;
                        break;
                    case "mainTexture":
                        value = command.mainTexture;
                        break;
                    default:
                        value = uniform.propertyInfo.GetValue(material);
                        break;
                }
                if (uniform.propertyInfo.PropertyType == typeof(Texture))
                {
                    if (!BindTexture(textureIndex, command.mainTexture))
                    {
                        continue;
                    }
                    extra = textureIndex;
                    textureIndex++;
                }
                SetProgramUniform(program, uniform, value, extra);
            }

            // draw
            OpenGL.PrimitiveType primitiveType = ConvertPrimitiveTypeToOpenGL(mesh.primitiveType);
            Gl.DrawElements(primitiveType, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public override void UpdateTexture(Texture texture, uint x, uint y, uint blockWidth, uint blockHeight, Color[] pixels)
        {
            if (!textureCache.TryGetValue(texture, out uint textureID))
            {
                throw new Exception("an unregisted texture is requesting for update");
            }
            Gl.BindTexture(TextureTarget.Texture2d, textureID);
            Gl.TexSubImage2D(TextureTarget.Texture2d, 0, (int)x, (int)y, (int)blockWidth, (int)blockHeight, PixelFormat.Rgba, PixelType.Float, pixels);
        }
    }
}
