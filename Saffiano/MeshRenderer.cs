namespace Saffiano
{
    public class LambertMaterial : ScriptingMaterial
    {
        [Uniform]
        public Matrix4x4 u_MVP { get; set; }

        [Uniform]
        public Matrix4x4 u_MV { get; set; }

        void VertexShader(
            [Attribute(location: 0)] in Vector3 a_position,
            [Attribute(location: 1)] in Vector3 a_normal,
            [Attribute(location: 2)] in Vector2 a_texcoord,
            out Vector4 gl_Position,
            out Vector4 a_color
        )
        {
            Vector3 lightDirection = new Vector3(0, 0, 1);
            Vector3 normal = (u_MV * new Vector4(a_normal, 1.0f)).xyz.normalized;
            Vector3 lightColor = new Vector3(1, 1, 1);
            gl_Position = u_MVP * new Vector4(a_position, 1.0f);
            a_color = new Vector4(lightColor * Mathf.Max(Vector3.Dot(normal, lightDirection), 0), 1);
        }

        void FragmentShader(
            in Vector4 a_color,
            out Vector4 color
        )
        {
            color = a_color;
        }
    }

    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter => gameObject.GetComponent<MeshFilter>();

        public Material material { get; set; } = new LambertMaterial();

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null || material == null)
            {
                return;
            }
            var command = new Command()
            {
                projection = Rendering.projection,
                transform = transform.ToRenderingMatrix(Rendering.device.coordinateSystem),
                mesh = meshFilter.mesh,
                shader = material.shader,
            };
            Rendering.Draw(command);
        }
    }
}
