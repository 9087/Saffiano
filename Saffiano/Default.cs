namespace Saffiano
{
    public partial class Resources
    {
        public static class Default
        {
            public static class Material
            {
                public class Basic : ScriptableMaterial
                {
                    void VertexShader(
                        [Attribute(location: 0)] in Vector3 a_position,
                        [Attribute(location: 1)] in Vector3 a_normal,
                        [Attribute(location: 2)] in Vector2 a_texcoord,
                        [Attribute(location: 3)] in Color a_color,
                        out Vector4 gl_Position,
                        out Vector2 v_texcoord,
                        out Color v_color
                    )
                    {
                        gl_Position = mvp * new Vector4(a_position, 1.0f);
                        v_texcoord = a_texcoord;
                        v_color = a_color;
                    }

                    void FragmentShader(
                        in Vector2 v_texcoord,
                        in Color v_color,
                        out Color color
                    )
                    {
                        color = (Color)((Vector4)mainTexture.Sample(v_texcoord) * (Vector4)v_color);
                    }
                }

                public class Lambert : ScriptableMaterial
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

            public static class Mesh
            {
                public class Plane : Saffiano.Mesh
                {
                    public Plane() : base()
                    {
                        vertices = new Vector3[] { new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), };
                        indices = new uint[] { 0, 1, 2, 2, 3, 0, };
                        uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), };
                        colors = new Color[] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1) };
                        primitiveType = PrimitiveType.Triangles;
                        normals = RecalculateNormals();
                    }
                }
            }
        }
    }
}