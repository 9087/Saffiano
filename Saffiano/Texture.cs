using FreeImageAPI;
using Saffiano.Rendering;
using System;
using System.Linq;

namespace Saffiano
{
    [Shader(OpenGL: "sampler2D")]
    public class Texture : Asset
    {
        private Color[] pixels { get; set; } = null;

        internal Atlas.ReferenceDescriptor referenceDescriptor { get; set; }

        internal Atlas atlas
        {
            get => referenceDescriptor != null ? referenceDescriptor.atlas : null;
        }

        private uint x = 0;
        private uint y = 0;

        public uint width { get; private set; }

        public uint height { get; private set; }

        internal bool registered { get; set; }

        internal Vector2 uvBottomRight
        {
            get
            {
                if (this.atlas != null)
                {
                    return new Vector2((float)(this.x + this.width) / this.atlas.width, (float)(this.y) / this.atlas.height);
                }
                else
                {
                    return new Vector2(1.0f, 0);
                }
            }
        }

        internal Vector2 uvBottomLeft
        {
            get
            {
                if (this.atlas != null)
                {
                    return new Vector2((float)(this.x) / this.atlas.width, (float)(this.y) / this.atlas.height);
                }
                else
                {
                    return new Vector2(0, 0);
                }
            }
        }

        internal Vector2 uvTopRight
        {
            get
            {
                if (this.atlas != null)
                {
                    return new Vector2((float)(this.x + this.width) / this.atlas.width, (float)(this.y + this.height) / this.atlas.height);
                }
                else
                {
                    return new Vector2(1.0f, 1.0f);
                }
            }
        }

        internal Vector2 uvTopLeft
        {
            get
            {
                if (this.atlas != null)
                {
                    return new Vector2((float)(this.x) / this.atlas.width, (float)(this.y + this.height) / this.atlas.height);
                }
                else
                {
                    return new Vector2(0, 1.0f);
                }
            }
        }

        internal Texture(Atlas.ReferenceDescriptor referenceDescriptor)
        {
            this.referenceDescriptor = referenceDescriptor;
            this.x = referenceDescriptor.x;
            this.y = referenceDescriptor.y;
            this.width = referenceDescriptor.width;
            this.height = referenceDescriptor.height;
        }

        public Texture(uint width, uint height)
        {
            this.width = width;
            this.height = height;
            this.pixels = Enumerable.Repeat(new Color(0, 0, 0), (int)(width * height)).ToArray();
        }

        internal Texture(string filePath) : base(filePath)
        {
        }

        ~Texture()
        {
            if (this.registered)
            {
                throw new System.Exception();
            }
            if (this.referenceDescriptor != null)
            {
                this.referenceDescriptor.atlas.Free(this);
                this.pixels = null;
                this.referenceDescriptor = null;
            }
        }

        [FileFormat]
        private void PNG(string filePath)
        {
            FIBITMAP bitmap = FreeImage.LoadEx(filePath);
            width = FreeImage.GetWidth(bitmap);
            height = FreeImage.GetHeight(bitmap);
            pixels = new Color[height * width];
            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    RGBQUAD value;
                    FreeImage.GetPixelColor(bitmap, x, y, out value);
                    uint index = y * width + x;
                    pixels[index].r = value.rgbRed / 255.0f;
                    pixels[index].g = value.rgbGreen / 255.0f;
                    pixels[index].b = value.rgbBlue / 255.0f;
                    pixels[index].a = value.rgbReserved / 255.0f;
                }
            }
            FreeImage.UnloadEx(ref bitmap);
        }

        public void SetPixels(Color[] pixels)
        {
            if (atlas != null)
            {
                atlas.SetPixels(x, y, width, height, pixels);
                return;
            }
            else
            {
                this.pixels = pixels;
                if (this.registered)
                {
                    RenderPipeline.UpdateTexture(this, 0, 0, this.width, this.height, this.pixels);
                }
            }
        }

        public Color[] GetPixels()
        {
            return this.pixels;
        }

        public virtual void SetPixels(uint x, uint y, uint blockWidth, uint blockHeight, Color[] colors)
        {
            if (atlas != null)
            {
                atlas.SetPixels(x + this.x, y + this.y, width, height, colors);
                return;
            }
            for (uint yi = 0; yi < blockHeight; yi++)
            {
                for (uint xi = 0; xi < blockWidth; xi++)
                {
                    uint sourceIndex = yi * blockWidth + xi;
                    uint targetIndex = (y + yi) * width + (xi + x);
                    pixels[targetIndex] = colors[sourceIndex];
                }
            }
            if (this.registered)
            {
                RenderPipeline.UpdateTexture(this, x, y, blockWidth, blockHeight, colors);
            }
        }

        internal virtual void OnRegister()
        {
            registered = true;
        }

        internal virtual void OnUnregister()
        {
            registered = false;
        }

        [Shader(OpenGL: "texture({0}, {1})")]
        public Color Sample(Vector2 coordinate)
        {
            throw new Exception("this method is just defined for scripting material call");
        }
    }
}
