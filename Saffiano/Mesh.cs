using System;
using System.IO;
using System.Dynamic;
using System.Collections.Generic;

namespace Saffiano
{
    public class Mesh : Asset, IDisposable
    {
        internal Vector3[] vertices
        {
            get;
            private set;
        }

        internal Vector3[] normals
        {
            get;
            private set;
        }

        internal Vector2[] uv
        {
            get;
            private set;
        }

        internal uint[] indices
        {
            get;
            private set;
        }

        internal PrimitiveType primitiveType
        {
            get;
            private set;
        }

        public Mesh(string filePath) : base()
        {
            switch (Path.GetExtension(filePath).ToUpper())
            {
                case ".PLY":
                    PLY(filePath);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
        }

        private void PLY(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
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
    }
}
