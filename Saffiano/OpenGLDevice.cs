using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saffiano
{
    internal class VertexData
    {
        public uint vao { get; internal set; }

        public uint ebo { get; internal set; }

        public Dictionary<AttributeType, uint> buffers { get; internal set; } = new Dictionary<AttributeType, uint>();
    }

    internal class VertexCache : Cache<Mesh, VertexData>
    {
        private uint CreateAndInitializeVertexAttribArray<T>(AttributeType location, T[] array)
        {
            uint buffer = Gl.GenBuffer();
            Gl.EnableVertexAttribArray((uint)location);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(T)) * array.Length), array, BufferUsage.StaticDraw);
            Gl.VertexAttribPointer((uint)location, Marshal.SizeOf(typeof(T)) / Marshal.SizeOf(typeof(float)), VertexAttribType.Float, false, 0, IntPtr.Zero);
            return buffer;
        }

        protected override VertexData OnRegister(Mesh mesh)
        {
            VertexData vertexData = new VertexData();

            vertexData.vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vertexData.vao);

            if (mesh.vertices == null || mesh.indices == null)
            {
                throw new Exception();
            }

            foreach (var attributeData in Mesh.attributeDatas)
            {
                var array = attributeData.GetArray(mesh);
                if (array == null)
                {
                    continue;
                }
                uint buffer = Gl.GenBuffer();
                vertexData.buffers.Add(attributeData.attributeType, buffer);
                Gl.EnableVertexAttribArray((uint)attributeData.attributeType);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                int length = attributeData.GetArrayLength(array);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(attributeData.itemType) * length), array, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)attributeData.attributeType, Marshal.SizeOf(attributeData.itemType) / Marshal.SizeOf(typeof(float)), VertexAttribType.Float, false, 0, IntPtr.Zero);
            }

            vertexData.ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, vertexData.ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * mesh.indices.Length), mesh.indices, BufferUsage.StaticDraw);

            Gl.BindVertexArray(0);

            return vertexData;
        }

        protected override void OnUnregister(Mesh mesh)
        {
            Gl.DeleteBuffers(this[mesh].ebo);
            Gl.DeleteBuffers(this[mesh].buffers.Values.ToArray());
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
            if (success == Gl.FALSE)
            {
                // compile error
                Gl.GetShader(shader, ShaderParameterName.InfoLogLength, out int length);
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
            Gl.DeleteShader(this[key].vs);
            Gl.DeleteShader(this[key].fs);
            Gl.DeleteProgram(this[key].program);
        }
    }

    internal class FrameBufferData
    {
        public uint fbo { get; internal set; }

        public uint depthTexture { get; internal set; }
    }

    internal class FrameBuffer
    {
        public RenderTexture renderTexture { get; private set; } = null;

        public FrameBuffer(RenderTexture renderTexture)
        {
            this.renderTexture = renderTexture;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FrameBuffer))
            {
                return false;
            }
            return this.renderTexture == (obj as FrameBuffer).renderTexture;
        }

        public override int GetHashCode()
        {
            return this.renderTexture.GetHashCode();
        }
    }

    internal class FrameBufferCache : Cache<FrameBuffer, FrameBufferData>
    {
        protected override FrameBufferData OnRegister(FrameBuffer key)
        {
            uint fbo = Gl.CreateFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, (device as OpenGLDevice).textureCache.TryRegister(key.renderTexture), 0);
            uint depthTexture = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, depthTexture);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent32, (int)key.renderTexture.width, (int)key.renderTexture.height, 0, PixelFormat.DepthComponent, PixelType.Float, 0);
            Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2d, depthTexture, 0);
            return new FrameBufferData() { fbo = fbo, depthTexture = depthTexture };
        }

        protected override void OnUnregister(FrameBuffer key)
        {
            Gl.DeleteTextures(this[key].depthTexture);
            Gl.DeleteFramebuffers(this[key].fbo);
        }
    }

    internal class OpenGLDevice : Device
    {
        private DeviceContext deviceContext;
        private IntPtr glContext;

        internal List<IDeperactedClean> caches = new List<IDeperactedClean>();
        internal VertexCache vertexCache;
        internal TextureCache textureCache;
        internal GPUProgramCache shaderCache;
        internal FrameBufferCache frameBufferCache;
        internal Camera currentCamera;

        static OpenGLDevice()
        {
            Gl.Initialize();
        }

        private T CreateCache<T>() where T : IDeperactedClean, new()
        {
            var cache = new T();
            cache.SetDevice(this);
            caches.Add(cache);
            return cache;
        }

        private void InitializeCache()
        {
            vertexCache = CreateCache<VertexCache>();
            textureCache = CreateCache<TextureCache>();
            shaderCache = CreateCache<GPUProgramCache>();
            frameBufferCache = CreateCache<FrameBufferCache>();
        }

        public OpenGLDevice(Win32Window window)
        {
            InitializeCache();

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

        public override void Clear(Color backgroundColor)
        {
            Gl.ClearColor(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void BeginScene(Camera camera)
        {
            currentCamera = camera;
            MakeCurrent();
            var renderTexture = camera.TargetTexture;
            if (renderTexture is null)
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferCache.TryRegister(new FrameBuffer(renderTexture)).fbo);
            }
        }

        public override void EndScene()
        {
            Gl.Flush();
            currentCamera = null;
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
            if (command.depthTest)
            {
                Gl.Enable(EnableCap.DepthTest);
                Gl.DepthFunc(DepthFunction.Less);
            }
            else
            {
                Gl.Disable(EnableCap.DepthTest);
            }
            if (command.blend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }
            Gl.Enable(EnableCap.CullFace);

            // apply shader
            var material = command.material;
            var shader = material.shader;
            if (currentCamera.replacementShaders.TryGetValue("", out GPUProgram replacementShader))
            {
                shader = replacementShader;
            }
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
                    if (!BindTexture(textureIndex, value as Texture))
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

        public override void Start()
        {
            caches.ForEach((x) => x.Start());
        }

        public override void End()
        {
            caches.ForEach((x) => x.End());
            SwapBuffers();
        }
    }
}
