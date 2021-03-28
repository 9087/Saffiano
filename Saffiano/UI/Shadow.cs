using System.Collections.Generic;

namespace Saffiano.UI
{
    public class Shadow : BaseMeshEffect
    {
        public Vector2 effectDistance { get; set; } = new Vector2(2, 2);

        public Color effectColor { get; set; } = new Color(0, 0, 0, 0.5f);

        public override void ModifyMesh(Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<uint> indices = new List<uint>();
            List<Vector2> uv = new List<Vector2>();
            List<Color> colors = null;

            // vertices
            foreach (var vertice in mesh.vertices)
            {
                var @new = vertice + new Vector3(effectDistance.x, effectDistance.y, 0);
                vertices.Add(@new);
            }
            vertices.AddRange(mesh.vertices);

            // indices
            indices.AddRange(mesh.indices);
            foreach (var indice in mesh.indices)
            {
                indices.Add(indice + (uint)mesh.vertices.Length);
            }

            // uv
            uv.AddRange(mesh.uv);
            uv.AddRange(mesh.uv);

            // color
            if (mesh.colors != null)
            {
                colors = new List<Color>();
                foreach (var color in mesh.colors)
                {
                    colors.Add(effectColor);
                }
                colors.AddRange(mesh.colors);
            }

            mesh.vertices = vertices.ToArray();
            mesh.indices = indices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.colors = colors == null ? null : colors.ToArray();
        }
    }
}
