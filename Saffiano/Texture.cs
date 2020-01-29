using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Saffiano
{
    public class Texture : Asset
    {
        public Color[] pixels
        {
            get;
            private set;
        }

        public uint width
        {
            get;
            private set;
        }

        public uint height
        {
            get;
            private set;
        }

        public Texture(string filePath) : base(filePath)
        {
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
    }
}
