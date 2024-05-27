using Assimp;
using ChungusEngine.Vector;
using OpenGL;
using System.Runtime.InteropServices;

namespace ChungusEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex(Vec3 position, Vec3 normal, Vec2 uv)
    {
        public readonly Vec3 Position => position;
        public readonly Vec3 Normal => normal;
        public readonly Vec2 UV => uv;
    }

    public struct Texture(uint id, TextureType type, string path)
    {
        public readonly uint Id => id;
        public readonly TextureType Type => type;
        public readonly string Path => path;
    }

    public enum TexParamValues
    {
        GL_REPEAT = 0x2901,
        GL_LINEAR_MIPMAP_LINEAR = 0x2703,
        GL_LINEAR = 0x2601
    }

    public class Mesh : IDisposable
    {
        public List<Vertex> Vertices { get; set; }
        public List<uint> Indices { get; set; }
        public List<Texture> Textures { get; set; }

        private uint VAO;
        private uint VBO;
        private uint EBO;

        public Mesh(List<Vertex> vertices, List<uint> indices, List<Texture> textures)
        {
            Vertices = vertices;
            Indices = indices;
            Textures = textures;
            SetupMesh();
        }

        private void SetupMesh()
        {
            // Generate the ids for the VAO/VBO/EBO
            VAO = Gl.GenVertexArray();
            VBO = Gl.GenBuffer();
            EBO = Gl.GenBuffer();

            Gl.BindVertexArray(VAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            int VERTEX_SIZE = Marshal.SizeOf(typeof(Vertex));

            // Potential FIXME -> the Learn OpenGL code gives this function a pointer to the first item in the vector.
            // Does this do the same thing?
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertices.Count * VERTEX_SIZE), Vertices.ToArray(), BufferUsage.StaticDraw);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);                                        // same thing here...
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(Indices.Count * sizeof(uint)), Indices.ToArray(), BufferUsage.StaticDraw);

            // Expose position vector to shader program
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, VERTEX_SIZE, 0);

            // Expose vertex normal to shader program
            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, VERTEX_SIZE, 24);

            // Expose tex coordinates to shader program
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 2, VertexAttribType.Float, false, VERTEX_SIZE, 48);

/*            Gl.EnableVertexAttribArray(3);
            Gl.VertexAttribPointer(3, 3, VertexAttribType.Float, false, VERTEX_SIZE, Marshal.OffsetOf<Vertex>(nameof(Vertex.Tangent)));

            Gl.EnableVertexAttribArray(4);
            Gl.VertexAttribPointer(4, 3, VertexAttribType.Float, false, VERTEX_SIZE, Marshal.OffsetOf<Vertex>(nameof(Vertex.Bitangent)));*/

            Gl.BindVertexArray(0);
        }

        public void Draw(ShaderProgram shader)
        {
            uint DiffuseIdx = 1;
            uint SpecularIdx = 1;

            for (int i = 0; i < Textures.Count; i++)
            {
                TextureUnit textureIdx = TextureUnit.Texture0 + i;

                Gl.ActiveTexture(textureIdx);

                var currentTexture = Textures[i];

                switch (currentTexture.Type)
                {
                    case TextureType.Diffuse:
                        DiffuseIdx++;
                        break;
                    case TextureType.Specular:
                        SpecularIdx++;
                        break;
                }

                shader.SetInt($"material.{currentTexture.Type}.{(currentTexture.Type == TextureType.Diffuse ? DiffuseIdx : SpecularIdx)}", i);
                Gl.BindTexture(TextureTarget.Texture2d, Textures[i].Id);
            }

            Gl.ActiveTexture(TextureUnit.Texture0);

            Gl.BindVertexArray(VAO);
            Gl.DrawElements(OpenGL.PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
            Gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Gl.DeleteVertexArrays(VAO);
            Gl.DeleteBuffers(VBO, EBO);
        }
    }
}
