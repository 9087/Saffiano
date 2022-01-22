using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Saffiano.Rendering.OpenGL
{
    internal class VertexData
    {
        public uint vao { get; internal set; }

        public uint ebo { get; internal set; }

        public Dictionary<AttributeType, uint> buffers { get; internal set; } = new Dictionary<AttributeType, uint>();
    }

    internal class VertexCache : Cache<Mesh, VertexData>
    {
        private uint CreateAndInitializeVertexAttribArray<T>(AttributeType location, T[] array)
        {
            uint buffer = Gl.GenBuffer();
            Gl.EnableVertexAttribArray((uint)location);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(typeof(T)) * array.Length), array, BufferUsage.StaticDraw);
            Gl.VertexAttribPointer((uint)location, Marshal.SizeOf(typeof(T)) / Marshal.SizeOf(typeof(float)), VertexAttribType.Float, false, 0, IntPtr.Zero);
            return buffer;
        }

        protected override VertexData OnRegister(Mesh mesh)
        {
            VertexData vertexData = new VertexData();

            vertexData.vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vertexData.vao);

            if (mesh.vertices == null || mesh.indices == null)
            {
                throw new Exception();
            }

            foreach (var attributeData in Mesh.attributeDatas)
            {
                var array = attributeData.GetArray(mesh);
                if (array == null)
                {
                    continue;
                }
                uint buffer = Gl.GenBuffer();
                vertexData.buffers.Add(attributeData.attributeType, buffer);
                Gl.EnableVertexAttribArray((uint)attributeData.attributeType);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                int length = attributeData.GetArrayLength(array);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(attributeData.itemType) * length), array, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer((uint)attributeData.attributeType, Marshal.SizeOf(attributeData.itemType) / Marshal.SizeOf(typeof(float)), VertexAttribType.Float, false, 0, IntPtr.Zero);
            }

            vertexData.ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, vertexData.ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * mesh.indices.Length), mesh.indices, BufferUsage.StaticDraw);

            Gl.BindVertexArray(0);

            return vertexData;
        }

        protected override void OnUnregister(Mesh mesh)
        {
            Gl.DeleteBuffers(this[mesh].ebo);
            Gl.DeleteBuffers(this[mesh].buffers.Values.ToArray());
            Gl.DeleteVertexArrays(this[mesh].vao);
        }
    }
}
