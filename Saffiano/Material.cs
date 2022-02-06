using System.Collections.Generic;

namespace Saffiano
{
    public enum ShaderType
    {
        VertexShader = 0,
        GeometryShader = 36313,
        FragmentShader = 35632,
    }

    public enum CullMode : uint
    {
        Off = 0,
        Front = 1,
        Back = 2,
        FrontAndBack = 3,
    }

    public enum ZTest : uint
    {
        Less = 0,
        LEqual = 1,
        Equal = 2,
        GEqual = 3,
        Greater = 4,
        NotEqual = 5,
        Always = 6,
    }

    public enum BlendFactor : uint
    {
        Off,
        One,
        Zero,
        SrcColor,
        SrcAlpha,
        DstColor,
        DstAlpha,
        OneMinusSrcColor,
        OneMinusSrcAlpha,
        OneMinusDstColor,
        OneMinusDstAlpha,
    }

    public struct Blend
    {
        private readonly BlendFactor _source;
        private readonly BlendFactor _destination;

        public BlendFactor source => _source;

        public BlendFactor destination => _destination;

        public Blend(BlendFactor source, BlendFactor destination)
        {
            Debug.Assert(
                (source == BlendFactor.Off && destination == BlendFactor.Off)
                ||
                (source != BlendFactor.Off && destination != BlendFactor.Off)
            );
            this._source = source;
            this._destination = destination;
        }

        public static Blend transparency => new Blend(BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);

        public static Blend additive => new Blend(BlendFactor.One, BlendFactor.One);

        public static Blend off => new Blend(BlendFactor.Off, BlendFactor.Off);

        public override bool Equals(object obj)
        {
            if (!(obj is Blend))
            {
                return false;
            }
            var blend = (Blend)obj;
            return this._source == blend._source && this._destination == blend._destination;
        }

        public static bool operator ==(Blend x, Blend y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Blend x, Blend y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            int hashCode = -25759443;
            hashCode = hashCode * -1521134295 + _source.GetHashCode();
            hashCode = hashCode * -1521134295 + _destination.GetHashCode();
            return hashCode;
        }
    }

    public abstract class Material : Asset
    {
        public virtual Saffiano.Rendering.GPUProgram shader { get; }

        internal HashSet<Uniform> uniforms { get; set; }

        public virtual CullMode cullMode { get; set; } = CullMode.Back;

        public virtual ZTest zTest { get; set; } = ZTest.Less;

        public virtual Blend blend { get; set; } = Blend.off;

        protected Material()
        {
        }
    }
}
