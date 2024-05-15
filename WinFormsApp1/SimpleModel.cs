using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine
{
    public class SimpleModel
    {
        public string Name { get; set; }
        public Matrix4x4f ModelTransform { get; set; }
        public float[] Vertices { get; set; }
        public float[] Colors { get; set; }
        public ushort[] Indices { get; set; }

        public uint VertexArrayId { get; set; }

        public SimpleModel(string name, Matrix4x4f modelTransform, float[] vertices, float[] colors, ushort[] indices)
        {
            Name = name;
            ModelTransform = modelTransform;
            Vertices = vertices;
            Colors = colors;
            Indices = indices;
            VertexArrayId = BindModel();
        }

        private uint BindModel()
        {
            uint id = Gl.GenVertexArray();
            Gl.BindVertexArray(id);

            uint CubeVBO = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, CubeVBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, Convert.ToUInt32(sizeof(float) * Vertices.Length), Vertices, BufferUsage.StaticDraw);

            uint IndicesBufferObj = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBufferObj);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, Convert.ToUInt32(sizeof(ushort) * Indices.Length), Indices, BufferUsage.StaticDraw);

            // Position attrib
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, 0);
            Gl.EnableVertexAttribArray(0);

            Gl.BindVertexArray(0);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return id;
        }

        public void DrawModel(ShaderProgram shaderProgram)
        {
            Gl.UniformMatrix4f(shaderProgram.ModelMatrix, 1, false, ModelTransform);
            Gl.BindVertexArray(VertexArrayId);
            // FIXME try to figure out how to compute this dynamically, this will break for models
            // other than the cube I got
            Gl.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedShort, null);
            Gl.BindVertexArray(0);  
        }
    }
}
