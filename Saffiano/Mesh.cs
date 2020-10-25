using System;
using System.IO;
using System.Collections.Generic;

namespace Saffiano
{
    public class Mesh : Asset, IDisposable
    {
        internal Vector3[] vertices
        {
            get;
            set;
        }

        internal Vector3[] normals
        {
            get;
            set;
        }

        internal Vector2[] uv
        {
            get;
            set;
        }

        internal uint[] indices
        {
            get;
            set;
        }

        internal PrimitiveType primitiveType
        {
            get;
            set;
        }

        internal Mesh(string filePath) : base(filePath)
        {
            if (normals == null)
            {
                normals = RecalculateNormals();
            }
        }

        internal Mesh() : base()
        {
        }

        public void Dispose()
        {
        }

        [FileFormat]
        private void PLY(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            PLY ply = new PLY(fileStream);
            var vertexDatas = ply.data.vertex;
            var faceDatas = ply.data.face;
            vertices = new Vector3[vertexDatas.Length];
            for (uint i = 0; i < vertexDatas.Length; i++)
            {
                vertices[i] = new Vector3(vertexDatas[i].x, vertexDatas[i].y, vertexDatas[i].z);
            }
            var faceElement = ply.FindElementByName("face");
            var indicesPropertyName = faceElement.GetPropertyByIndex(0).name;
            var firstFaceData = faceDatas[0] as IDictionary<string, dynamic>;
            uint indicesLength = (uint)firstFaceData[indicesPropertyName].Length;
            primitiveType = PrimitiveType.Unknown;
            switch (indicesLength)
            {
                case 3:
                    primitiveType = PrimitiveType.Triangles;
                    break;
                case 4:
                    primitiveType = PrimitiveType.Quads;
                    break;
                default:
                    throw new NotImplementedException();
            }
            indices = new uint[faceDatas.Length * indicesLength];
            for (uint i = 0; i < faceDatas.Length; i++)
            {
                var faceData = faceDatas[i] as IDictionary<string, dynamic>;
                var indicesData = faceData[indicesPropertyName];
                if (indicesData.Length != indicesLength)
                {
                    throw new FileFormatException();
                }
                for (uint j = 0; j < indicesData.Length; j++)
                {
                    indices[i * indicesLength + j] = (uint)indicesData[j];
                }
            }
            fileStream.Close();
            GC.SuppressFinalize(fileStream);
        }

        public Vector3[] RecalculateNormals()
        {
            Dictionary<uint, List<Vector3>> normalsMap = new Dictionary<uint, List<Vector3>>();
            for (uint i = 0; i < this.indices.Length / 3; i++)
            {
                Vector3 a = this.vertices[this.indices[i * 3 + 0]];
                Vector3 b = this.vertices[this.indices[i * 3 + 1]];
                Vector3 c = this.vertices[this.indices[i * 3 + 2]];
                Vector3 normal = Vector3.Cross(a - b, a - c);
                for (uint j = 0; j < 3; j++)
                {
                    uint index = this.indices[i * 3 + j];
                    if (!normalsMap.ContainsKey(index))
                    {
                        normalsMap.Add(index, new List<Vector3>());
                    }
                    normalsMap[index].Add(normal);
                }
            }
            Vector3[] result = new Vector3[this.vertices.Length];
            foreach (KeyValuePair<uint, List<Vector3>> kv in normalsMap)
            {
                Vector3 sum = Vector3.zero;
                foreach (Vector3 v in kv.Value)
                {
                    sum += v;
                }
                result[kv.Key] = (sum / (float)(kv.Value.Count)).normalized;
            }
            return result;
        }
    }
}
