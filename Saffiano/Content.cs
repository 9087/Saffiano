namespace Saffiano.Content
{
    namespace Material
    {
        public class Basic : ScriptingMaterial
        {
            void VertexShader(
                [Attribute(location: 0)] in Vector3 a_position,
                [Attribute(location: 1)] in Vector3 a_normal,
                [Attribute(location: 2)] in Vector2 a_texcoord,
                out Vector4 gl_Position,
                out Vector2 v_texcoord
            )
            {
                gl_Position = mvp * new Vector4(a_position, 1.0f);
                v_texcoord = a_texcoord;
            }

            void FragmentShader(
                in Vector2 v_texcoord,
                out Vector4 color
            )
            {
                color = texture.Sample(v_texcoord);
            }
        }

        public class Lambert : ScriptingMaterial
        {
            [Uniform]
            public Vector3 directionLight { get; set; } = new Vector3(1, 1, 1);

            [Uniform]
            public Color directionLightColor { get; set; } = new Color(1, 0.956863f, 0.839216f);

            void VertexShader(
                [Attribute(location: 0)] in Vector3 a_position,
                [Attribute(location: 1)] in Vector3 a_normal,
                [Attribute(location: 2)] in Vector2 a_texcoord,
                out Vector4 gl_Position,
                out Color a_color
            )
            {
                Vector3 normal = (mv * new Vector4(a_normal, 1.0f)).xyz.normalized;
                gl_Position = mvp * new Vector4(a_position, 1.0f);
                a_color = (Color)((Vector4)directionLightColor * Mathf.Max(Vector3.Dot(normal, directionLight), 0));
            }

            void FragmentShader(
                in Vector4 a_color,
                out Vector4 color
            )
            {
                color = a_color;
            }
        }
    }
}