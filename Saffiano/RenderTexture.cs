namespace Saffiano
{
    public class RenderTexture : Texture
    {
        public RenderTexture(uint width, uint height, bool multisampling = false) : base(width, height, multisampling)
        {
        }
    }
}
