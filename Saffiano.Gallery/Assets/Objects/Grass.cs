using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Gallery.Assets.Objects
{
    class GeometryShaderVertex : Vertex
    {
        public GeometryShaderVertex(Vector4 gl_Position, Vector4 v_position, Vector2 v_uv) : base(gl_Position) { }
    }

    class GrassMaterial : ShadowMappingPhong
    {
        public override CullMode cullMode => CullMode.Off;

        [Uniform]
        public float minThinness => 0.04f;

        [Uniform]
        public float maxThinness => 0.06f;

        [Uniform]
        public float minHeight => 0.4f;

        [Uniform]
        public float maxHeight => 0.6f;

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
            [OutputMode(OutputMode.TriangleStrip, 64)] Output<Vertex> output
        )
        {
            var position = input[0].gl_Position;

            if (position.xz.magnitude > 0.5f)
            {
                return;
            }

            var random = Mathf.Random(position.xz);
            var rotation = Quaternion.Euler(new Vector3(0, Mathf.PI * 2.0f * random, 0));

            random = Mathf.Random(new Vector2(random, position.x));
            var thinness = minThinness + (maxThinness - minThinness) * random;

            random = Mathf.Random(new Vector2(random, position.x));
            var height = minHeight + (maxHeight - minHeight) * random;

            float angle = 0;
            float segment = 0;
            float height_0 = 0;
            float height_1 = 0;
            float thinness_0 = 0.5f * thinness;
            float thinness_1 = 0;
            Vector3 rig_0 = new Vector3(0, 0, 0);
            Vector3 rig_1 = new Vector3(0, 0, 0);
            Vector3 a = new Vector3(0, 0, 0);
            Vector3 b = new Vector3(0, 0, 0);
            Vector3 c = new Vector3(0, 0, 0);
            Vector3 d = new Vector3(0, 0, 0);

            for (float i = 1; i <= 4; i++)
            {
                segment = (0.5f - ((float)i) * 0.1f) * height;
                height_1 = height_0 + segment;
                thinness_1  = (0.5f - ((float)i) * 0.125f) * thinness;
                angle = 9.0f * Mathf.Deg2Rad * i;
                rig_1 = rig_0 + new Vector3(0, Mathf.Cos(angle) * segment, Mathf.Sin(angle) * segment);

                a = rig_0 - new Vector3(thinness_0, 0, 0);
                b = rig_0 + new Vector3(thinness_0, 0, 0);
                c = rig_1 - new Vector3(thinness_1, 0, 0);
                d = rig_1 + new Vector3(thinness_1, 0, 0);

                var v_a = position + new Vector4(rotation * a, 0);
                var v_b = position + new Vector4(rotation * b, 0);
                var v_c = position + new Vector4(rotation * c, 0);
                var v_d = position + new Vector4(rotation * d, 0);

                output.AddPrimitive(
                    new GeometryShaderVertex(mvp * v_a, v_a, new Vector2(0, height_0)),
                    new GeometryShaderVertex(mvp * v_b, v_b, new Vector2(1, height_0)),
                    new GeometryShaderVertex(mvp * v_d, v_d, new Vector2(1, height_1))
                );
                output.AddPrimitive(
                    new GeometryShaderVertex(mvp * v_a, v_a, new Vector2(0, height_0)),
                    new GeometryShaderVertex(mvp * v_d, v_d, new Vector2(1, height_1)),
                    new GeometryShaderVertex(mvp * v_c, v_c, new Vector2(0, height_1))
                );

                height_0 = height_1;
                rig_0 = rig_1;
                thinness_0 = thinness_1;
            }
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
        }
    }
}
