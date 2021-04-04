namespace Saffiano.FileFormats
{
    using FreeTypeSharp;
    using FreeTypeSharp.Native;
    using System;
    using System.Runtime.InteropServices;

    internal class TTF
    {
        private static FreeTypeLibrary library = new FreeTypeLibrary();
        internal FreeTypeFaceFacade face;
        private bool antialias = true;

        static TTF()
        {
        }

        public TTF(string filePath, uint size)
        {
            FT_Error error;
            error = FT.FT_New_Face(library.Native, filePath, 0, out IntPtr pFace);
            face = new FreeTypeFaceFacade(library, pFace);
            if (error != FT_Error.FT_Err_Ok)
            {
                throw new FreeTypeException(error);
            }
            if (error != FT.FT_Set_Pixel_Sizes(face.Face, 0, size))
            {
                throw new FreeTypeException(error);
            }
        }

        public CharacterInfo RequestCharacterInfo(int charactorCode)
        {
            var glyphIndex = FT.FT_Get_Char_Index(face.Face, (uint)charactorCode);
            if (glyphIndex == 0)
            {
                return null;
            }
            FT_Error error;
            error = FT.FT_Load_Glyph(face.Face, glyphIndex, FT.FT_LOAD_DEFAULT);
            if (error != FT_Error.FT_Err_Ok)
            {
                throw new FreeTypeException(error);
            }
            FT_Render_Mode renderMode = antialias ? FT_Render_Mode.FT_RENDER_MODE_NORMAL : FT_Render_Mode.FT_RENDER_MODE_MONO;
            IntPtr pGlyphSlot;
            unsafe
            {
                pGlyphSlot = (IntPtr)face.GlyphSlot;
            }
            error = FT.FT_Render_Glyph(pGlyphSlot, renderMode);
            if (error != FT_Error.FT_Err_Ok)
            {
                throw new FreeTypeException(error);
            }
            FT_GlyphSlotRec glyphSlot = (FT_GlyphSlotRec) Marshal.PtrToStructure(pGlyphSlot, typeof(FT_GlyphSlotRec));
            FT_Bitmap bitmap = glyphSlot.bitmap;

            if (bitmap.buffer == IntPtr.Zero)
            {
                return new CharacterInfo(this, charactorCode, new Vector2((int)glyphSlot.advance.x >> 6, (int)glyphSlot.advance.y >> 6));
            }

            Texture texture = Font.atlas.Allocate(bitmap.width, bitmap.rows);
            switch ((FT_Pixel_Mode) bitmap.pixel_mode)
            {
                case FT_Pixel_Mode.FT_PIXEL_MODE_GRAY:
                    {
                        uint length = bitmap.width * bitmap.rows;
                        byte[] buffer = new Byte[length];
                        Marshal.Copy(bitmap.buffer, buffer, 0, (int)length);
                        Color[] pixels = new Color[length];
                        int count = 0;
                        for (int y = (int)bitmap.rows - 1; y >= 0; y--)
                        {
                            for (int x = 0; x < bitmap.width; x++)
                            {
                                int index = y * (int)bitmap.width + x;
                                pixels[count] = new Color { r = 1, g = 1, b = 1, a = (float)buffer[index] / 255.0f };
                                count++;
                            }
                        }
                        texture.SetPixels(pixels);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return new CharacterInfo(
                this,
                charactorCode,
                new Vector2((float)glyphSlot.advance.x / (float)(1 << 6), (float)glyphSlot.advance.y / (float)(1 << 6)),
                texture,
                new Vector2(0, glyphSlot.bitmap_top)
            );
        }
    }
}
