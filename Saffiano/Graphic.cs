namespace Saffiano
{
    public class BasicMaterial : ScriptingMaterial
    {
        [Uniform]
        public Matrix4x4 u_MVP { get; set; }

        [Uniform]
        public Texture tex { get; set; }

        void VertexShader(
            [Attribute(location: 0)] in Vector3 a_position,
            [Attribute(location: 1)] in Vector3 a_normal,
            [Attribute(location: 2)] in Vector2 a_texcoord,
            out Vector4 gl_Position,
            out Vector2 v_texcoord
        )
        {
            gl_Position = u_MVP * new Vector4(a_position, 1.0f);
            v_texcoord = a_texcoord;
        }

        void FragmentShader(
            in Vector2 v_texcoord,
            out Vector4 color
        )
        {
            color = TextureSample(tex, v_texcoord);
        }
    }

    public abstract class Graphic : Behaviour
    {
        internal abstract Command CreateCommand(RectTransform rectTransform);
    }
}
