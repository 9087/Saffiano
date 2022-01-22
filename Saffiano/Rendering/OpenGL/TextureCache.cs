using OpenGL;
using System;

namespace Saffiano.Rendering.OpenGL
{
    internal class TextureCache : Cache<Texture, uint>
    {
        protected override uint OnRegister(Texture texture)
        {
            uint textureID = Gl.GenTexture();
            TextureTarget target = !texture.multisampling ? TextureTarget.Texture2d : TextureTarget.Texture2dMultisample;
            Gl.BindTexture(target, textureID);
            if (!texture.multisampling)
            {
                Gl.TexImage2D(target, 0, InternalFormat.Rgba, (int)texture.width, (int)texture.height, 0, PixelFormat.Rgba, PixelType.Float, texture.GetPixels());
            }
            else
            {
                Gl.TexImage2DMultisample(target, (int)QualitySettings.antiAliasing, InternalFormat.Rgba, (int)texture.width, (int)texture.height, true);
            }
            Gl.TexParameterI(target, TextureParameterName.TextureMagFilter, new int[] { Gl.NEAREST });
            Gl.TexParameterI(target, TextureParameterName.TextureMinFilter, new int[] { Gl.NEAREST });
            switch (texture.wrapMode)
            {
                case TextureWrapMode.Clamp:
                    Gl.TexParameterI(target, TextureParameterName.TextureWrapS, new int[] { Gl.CLAMP });
                    Gl.TexParameterI(target, TextureParameterName.TextureWrapT, new int[] { Gl.CLAMP });
                    break;
                case TextureWrapMode.Repeat:
                    Gl.TexParameterI(target, TextureParameterName.TextureWrapS, new int[] { Gl.REPEAT });
                    Gl.TexParameterI(target, TextureParameterName.TextureWrapT, new int[] { Gl.REPEAT });
                    break;
                default:
                    throw new NotImplementedException();
            }
            Gl.GenerateMipmap(target);
            texture.OnRegister();
            Gl.BindTexture(target, 0);
            return textureID;
        }

        protected override void OnUnregister(Texture texture)
        {
            texture.OnUnregister();
            Gl.DeleteTextures(this[texture]);
        }
    }
}
