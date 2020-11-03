﻿namespace Saffiano
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
                        [Attribute(AttributeType.Position)] in Vector3 a_position,
                        [Attribute(AttributeType.Normal)] in Vector3 a_normal,
                        [Attribute(AttributeType.TexCoord)] in Vector2 a_texcoord,
                        [Attribute(AttributeType.Color)] in Color a_color,
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
                        out Color f_color
                    )
                    {
                        f_color = (Color)((Vector4)mainTexture.Sample(v_texcoord) * (Vector4)v_color);
                    }
                }

                public class Lambert : ScriptableMaterial
                {
                    [Uniform]
                    public Vector3 directionLight { get; set; } = new Vector3(1, 1, 1);

                    [Uniform]
                    public Color directionLightColor { get; set; } = new Color(1, 0.956863f, 0.839216f);

                    void VertexShader(
                        [Attribute(AttributeType.Position)] in Vector3 a_position,
                        [Attribute(AttributeType.Normal)] in Vector3 a_normal,
                        [Attribute(AttributeType.TexCoord)] in Vector2 a_texcoord,
                        [Attribute(AttributeType.Color)] in Color a_color,
                        out Vector4 gl_Position,
                        out Color v_color
                    )
                    {
                        Vector3 normal = (mv * new Vector4(a_normal, 1.0f)).xyz.normalized;
                        gl_Position = mvp * new Vector4(a_position, 1.0f);
                        Vector4 color = (Vector4)directionLightColor;
                        v_color = (Color)new Vector4(color.xyz * Mathf.Max(Vector3.Dot(normal, directionLight), 0), 1);
                    }

                    void FragmentShader(
                        in Vector4 v_color,
                        out Vector4 f_color
                    )
                    {
                        f_color = v_color;
                    }
                }
            }

            public static class Mesh
            {
                public class Plane : Saffiano.Mesh
                {
                    public Plane() : base()
                    {
                        vertices = new Vector3[] { new Vector3(-1, 0, 1), new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(1, 0, 1), };
                        indices = new uint[] { 0, 1, 2, 2, 3, 0, };
                        uv = new Vector2[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), };
                        colors = new Color[] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), };
                        primitiveType = PrimitiveType.Triangles;
                        normals = RecalculateNormals();
                    }
                }
            }
        }
    }
}