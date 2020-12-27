using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saffiano
{
    public partial class Resources { public static partial class Default { public static class Mesh
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

        public class Sphere : Saffiano.Mesh
        {
            public Sphere() : base()
            {
                uint longitudeSize = 128;
                uint latitudeSize = longitudeSize >> 2;
                uint equatorialSize = longitudeSize;
                Vector3[] vertices = new Vector3[(latitudeSize * longitudeSize + 1) * 2 - equatorialSize];
                for (uint latitude = 0; latitude < latitudeSize; latitude++)
                {
                    for (uint longitude = 0; longitude < longitudeSize; longitude++)
                    {
                        float longitudeAngle = (float)longitude * 2.0f * Mathf.PI / (float)longitudeSize;
                        float latitudeAngle = (float)latitude * Mathf.PI * 0.5f / (float)latitudeSize;
                        Vector3 point = new Vector3(
                            Mathf.Cos(latitudeAngle) * Mathf.Cos(longitudeAngle),
                            Mathf.Sin(latitudeAngle),
                            Mathf.Cos(latitudeAngle) * Mathf.Sin(longitudeAngle)
                        );
                        uint index = latitude * longitudeSize + longitude;
                        vertices[index] = point;
                    }
                }
                vertices[latitudeSize * longitudeSize] = new Vector3(0, 1, 0);
                List<uint> indices = new List<uint>();
                for (uint latitude = 1; latitude < latitudeSize; latitude++)
                {
                    for (uint longitude = 0; longitude < longitudeSize; longitude++)
                    {
                        /*
                            * C-D
                            * |/|
                            * A-B
                            */
                        uint A = (latitude - 1) * longitudeSize + longitude;
                        uint B = (latitude - 1) * longitudeSize + (longitude + 1) % longitudeSize;
                        uint C = latitude * longitudeSize + longitude;
                        uint D = latitude * longitudeSize + (longitude + 1) % longitudeSize;
                        indices.Add(A);
                        indices.Add(B);
                        indices.Add(D);
                        indices.Add(A);
                        indices.Add(D);
                        indices.Add(C);
                    }
                }
                for (uint longitude = 0; longitude < longitudeSize; longitude++)
                {
                    /*
                        *   C
                        *  /|
                        * A-B
                        */
                    uint A = (latitudeSize - 1) * longitudeSize + longitude;
                    uint B = (latitudeSize - 1) * longitudeSize + (longitude + 1) % longitudeSize;
                    uint C = latitudeSize * longitudeSize;
                    indices.Add(A);
                    indices.Add(B);
                    indices.Add(C);
                }

                int offset = (int)((vertices.Length + equatorialSize) / 2 - equatorialSize);
                for (uint index = equatorialSize; index < offset + equatorialSize; index++)
                {
                    vertices[index + offset] = new Vector3(vertices[index].x, -vertices[index].y, vertices[index].z);
                }
                int count = indices.Count;
                for (int index = 0; index < count / 3; index++)
                {
                    indices.Add(indices[index * 3 + 0] >= equatorialSize ? (uint)(indices[index * 3 + 0] + offset) : indices[index * 3 + 0]);
                    indices.Add(indices[index * 3 + 2] >= equatorialSize ? (uint)(indices[index * 3 + 2] + offset) : indices[index * 3 + 2]);
                    indices.Add(indices[index * 3 + 1] >= equatorialSize ? (uint)(indices[index * 3 + 1] + offset) : indices[index * 3 + 1]);
                }
                this.vertices = vertices;
                this.indices = indices.ToArray();
                colors = Enumerable.Repeat(new Color(1, 1, 1, 1), vertices.Length).ToArray(); ;
                primitiveType = PrimitiveType.Triangles;
                normals = RecalculateNormals();
            }
        }
    }
    }}  // Resources.Default.Mesh
}
