using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Gallery.Assets.Objects
{
    class GeometryShaderVertex : Vertex
    {
        public GeometryShaderVertex(Vector4 gl_Position, Vector2 v_uv) : base(gl_Position) { }
    }

    class GrassMaterial : ScriptableMaterial
    {
        public override CullMode cullMode => CullMode.Off;

        [Uniform]
        public float minThinness => 0.09f;

        [Uniform]
        public float maxThinness => 0.11f;

        [Uniform]
        public float minHeight => 0.9f;

        [Uniform]
        public float maxHeight => 1.1f;

        public virtual void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.Normal)] Vector3 a_normal,
            out Vector4 gl_Position
        )
        {
            gl_Position = new Vector4(a_position, 1.0f);
        }

        [Tessellation(TessellationMode.Quads, new uint[] { 16, 16, 16, 16 }, new uint[] { 16, 16 }, pointMode: true)]
        public void TessellationShader(Input<Vertex> input, Vector4 gl_TessCoord, out Vector4 gl_Position)
        {
            var u = gl_TessCoord.x;
            var v = gl_TessCoord.y;
            var p0 = input[0].gl_Position;
            var p1 = input[2].gl_Position;
            gl_Position = p0 + new Vector4((p1.x - p0.x) * u, 0, 0, 0) + new Vector4(0, 0, (p1.z - p0.z) * v, 0);
        }

        public new virtual void GeometryShader(
            [InputMode(InputMode.Points)] Input<Vertex> input,
            [OutputMode(OutputMode.TriangleStrip, 6)] Output<Vertex> output
        )
        {
            var position = input[0].gl_Position;
            var random = Mathf.Random(position.xz);
            var rotation = Quaternion.Euler(new Vector3(0, Mathf.PI * 2.0f * random, 0));

            random = Mathf.Random(new Vector2(random, position.x));
            var thinness = minThinness + (maxThinness - minThinness) * random;

            random = Mathf.Random(new Vector2(random, position.x));
            var height = minHeight + (maxHeight - minHeight) * random;
            output.AddPrimitive(
                new GeometryShaderVertex(
                    mvp * (position + new Vector4(rotation * (new Vector3(0, 0, +0.5f * thinness)), 0)),
                    new Vector2(1, 0)
                ),
                new GeometryShaderVertex(
                    mvp * (position + new Vector4(rotation * (new Vector3(0, 0, -0.5f * thinness)), 0)),
                    new Vector2(0, 0)
                ),
                new GeometryShaderVertex(
                    mvp * (position + new Vector4(rotation * (new Vector3(0, 1.0f * height, 0)), 0)),
                    new Vector2(0, 1)
                )
            );
        }

        public virtual void FragmentShader(
            Vector2 v_uv,
            out Color f_color
        )
        {
            var top = new Vector4(0, 0.5f, 0, 1.0f);
            var bottom = new Vector4(0, 0.2f, 0, 1.0f);
            f_color = (Color)(top * v_uv.y + bottom * (1.0f - v_uv.y));
        }
    }

    public class Grass : GameObject
    {
        public Grass()
        {
            this.AddComponent<Transform>();

            var mesh = new Resources.Default.Mesh.Plane(new Vector2(1, 1));

            GameObject grass = new GameObject();
            grass.AddComponent<Transform>();
            grass.AddComponent<MeshFilter>().mesh = mesh;
            grass.AddComponent<MeshRenderer>().material = new GrassMaterial();
            grass.transform.parent = this.transform;

            GameObject plane = new GameObject();
            plane.AddComponent<Transform>();
            plane.AddComponent<MeshFilter>().mesh = mesh;
            plane.AddComponent<MeshRenderer>();
            plane.transform.parent = this.transform;
        }
    }
}
