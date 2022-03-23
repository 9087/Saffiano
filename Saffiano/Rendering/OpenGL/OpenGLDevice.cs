using _OpenGL = OpenGL;
using OpenGL;
using System;
using System.Collections.Generic;

namespace Saffiano.Rendering
{
    internal class OpenGLDevice : Device
    {
        static Dictionary<BlendFactor, BlendingFactor> BlendingFactors = new Dictionary<BlendFactor, BlendingFactor>
        {
            {BlendFactor.One, BlendingFactor.One},
            {BlendFactor.Zero, BlendingFactor.Zero},
            {BlendFactor.SrcColor, BlendingFactor.SrcColor},
            {BlendFactor.SrcAlpha, BlendingFactor.SrcAlpha},
            {BlendFactor.DstColor, BlendingFactor.DstColor},
            {BlendFactor.DstAlpha, BlendingFactor.DstAlpha},
            {BlendFactor.OneMinusSrcColor, BlendingFactor.OneMinusSrcColor},
            {BlendFactor.OneMinusSrcAlpha, BlendingFactor.OneMinusSrcAlpha},
            {BlendFactor.OneMinusDstColor, BlendingFactor.OneMinusDstColor},
            {BlendFactor.OneMinusDstAlpha, BlendingFactor.OneMinusDstAlpha},
        };

        private DeviceContext deviceContext;
        private IntPtr glContext;

        internal List<IDeperactedClean> caches = new List<IDeperactedClean>();
        internal Saffiano.Rendering.OpenGL.VertexCache vertexCache;
        internal Saffiano.Rendering.OpenGL.TextureCache textureCache;
        internal Saffiano.Rendering.OpenGL.ShaderCache shaderCache;
        internal Saffiano.Rendering.OpenGL.FrameBufferCache frameBufferCache;
        internal Camera currentCamera;

        internal Saffiano.Rendering.OpenGL.MultisamplingAntialiasing msaa;

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
            vertexCache = CreateCache<Saffiano.Rendering.OpenGL.VertexCache>();
            textureCache = CreateCache<Saffiano.Rendering.OpenGL.TextureCache>();
            shaderCache = CreateCache<Saffiano.Rendering.OpenGL.ShaderCache>();
            frameBufferCache = CreateCache<Saffiano.Rendering.OpenGL.FrameBufferCache>();
        }

        public OpenGLDevice(Win32Window window)
        {
            DeviceContext.DefaultAPI = Khronos.KhronosVersion.ApiGl;

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

            msaa = new Saffiano.Rendering.OpenGL.MultisamplingAntialiasing();
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
            var renderTexture = camera.targetTexture;
            if (renderTexture is null)
            {
                if (QualitySettings.antiAliasing != 1)
                {
                    msaa.BeginScene(this);
                }
            }
            else
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferCache.TryRegister(new Saffiano.Rendering.OpenGL.RenderTextureFrameBuffer(renderTexture)).fbo);
            }
        }

        public override void EndScene()
        {
            if (currentCamera.targetTexture is null)
            {
                if (QualitySettings.antiAliasing != 1)
                {
                    msaa.EndScene(this);
                }
            }
            else
            {
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            Gl.Flush();
            currentCamera = null;
        }

        public override void SetViewport(Viewport viewport)
        {
            base.SetViewport(viewport);
            Gl.Viewport((int)viewport.x, (int)viewport.y, (int)viewport.width, (int)viewport.height);
        }

        private static _OpenGL.PrimitiveType ConvertPrimitiveTypeToOpenGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Triangles:
                    return _OpenGL.PrimitiveType.Triangles;
                case PrimitiveType.Quads:
                    return _OpenGL.PrimitiveType.Quads;
                default:
                    throw new NotImplementedException();
            }
        }

        private void BindVertex(Mesh mesh)
        {
            if (mesh != null)
            {
                Gl.BindVertexArray(vertexCache.TryRegister(mesh).vao);
            }
            else
            {
                Gl.BindVertexArray(0);
            }
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
                    var type = value.GetType();
                    if (type.IsEnum)
                    {
                        Gl.Uniform1(location, (int)value);
                        break;
                    }
                    throw new NotImplementedException(string.Format("uniform type {0} is not implemented.", value.GetType()));
            }
        }

        private void UpdateUniforms(uint program, Material material, Command command)
        {
            if (material == null)
            {
                return;
            }
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
        }

        public override void Draw(Command command)
        {
            var mesh = command.mesh;
            BindVertex(mesh);
            Enable(EnableCap.Lighting, command.lighting);

            // apply shader
            var material = command.material;
            Material replacementMaterial = null;
            ShaderSourceData shaderSourceData = material.shader.shaderSourceData;
            var shader = material.shader;
            if (currentCamera.replacementMaterials.TryGetValue("", out var replacementMaterialInfo))
            {
                replacementMaterial = replacementMaterialInfo.material;
                shader = replacementMaterial.shader;
                shaderSourceData = shaderSourceData.Clone() as ShaderSourceData;
                foreach (var shaderType in replacementMaterialInfo.shaderTypes)
                {
                    shaderSourceData.Update(shaderType, replacementMaterial.shader.shaderSourceData.codes.GetValueOrDefault(shaderType, null));
                }
            }
            uint program = shaderCache.TryRegister(shaderSourceData).program;
            Gl.UseProgram(program);
            UpdateUniforms(program, material, command);
            UpdateUniforms(program, replacementMaterial, command);

            #region Blending

            if (shader.blend == Blend.off)
            {
                Gl.Disable(EnableCap.Blend);
            }
            else
            {
                Gl.Enable(EnableCap.Blend);
                var sourceFactor = BlendingFactors.GetValueOrDefault(shader.blend.source, BlendingFactor.One);
                var destinationFactor = BlendingFactors.GetValueOrDefault(shader.blend.destination, BlendingFactor.One);
                Gl.BlendFunc(sourceFactor, destinationFactor);
            }

            #endregion

            #region Cull mode

            if (shader.cullMode == CullMode.Off)
            {
                Gl.Disable(EnableCap.CullFace);
            }
            else
            {
                Gl.Enable(EnableCap.CullFace);
                switch (shader.cullMode)
                {
                    case CullMode.Front:
                        Gl.CullFace(CullFaceMode.Front);
                        break;
                    case CullMode.Back:
                        Gl.CullFace(CullFaceMode.Back);
                        break;
                    case CullMode.FrontAndBack:
                        Gl.CullFace(CullFaceMode.FrontAndBack);
                        break;
                }
            }

            #endregion

            #region Depth test

            if (shader.zTest == ZTest.Always)
            {
                Gl.Disable(EnableCap.DepthTest);
            }
            else
            {
                Gl.Enable(EnableCap.DepthTest);
                switch (shader.zTest)
                {
                    case ZTest.Less:
                        Gl.DepthFunc(DepthFunction.Less);
                        break;
                    case ZTest.LEqual:
                        Gl.DepthFunc(DepthFunction.Lequal);
                        break;
                    case ZTest.Equal:
                        Gl.DepthFunc(DepthFunction.Equal);
                        break;
                    case ZTest.GEqual:
                        Gl.DepthFunc(DepthFunction.Gequal);
                        break;
                    case ZTest.Greater:
                        Gl.DepthFunc(DepthFunction.Greater);
                        break;
                    case ZTest.NotEqual:
                        Gl.DepthFunc(DepthFunction.Notequal);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            #endregion

            // draw
            var tessellationConfiguration = shaderSourceData.tessellationConfiguration;
            if (tessellationConfiguration != null)
            {
                Gl.PatchParameter(PatchParameterName.PatchVertices, tessellationConfiguration.patchVerticesCount);
                switch (tessellationConfiguration.drawType)
                {
                    case TessellationDrawType.Element:
                        Gl.DrawElements(_OpenGL.PrimitiveType.Patches, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                        break;
                    case TessellationDrawType.Vertics:
                        Gl.DrawArrays(_OpenGL.PrimitiveType.Patches, 0, mesh.vertices.Length);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                _OpenGL.PrimitiveType primitiveType = ConvertPrimitiveTypeToOpenGL(mesh.primitiveType);
                Gl.DrawElements(primitiveType, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            BindVertex(null);
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
