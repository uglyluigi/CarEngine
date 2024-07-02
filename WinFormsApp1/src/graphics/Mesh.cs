using Assimp;
using OpenGL;

namespace ChungusEngine.Graphics
{

    public struct Texture(uint id, TextureType type, string path)
    {
        public readonly uint Id => id;
        public readonly TextureType Type => type;
        public readonly string Path => path;
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
            (VAO, VBO, EBO) = OGLServices.Buffer([.. Vertices], [.. Indices]);
        }

        public void Draw(ShaderProgram shader)
        {
            uint DiffuseIdx = 1;
            uint SpecularIdx = 1;
            uint NormalIdx = 1;
            uint HeightIdx = 1;

            string number = "";

            for (int i = 0; i < Textures.Count; i++)
            {
                TextureUnit textureIdx = TextureUnit.Texture0 + i;

                Gl.ActiveTexture(textureIdx);

                var currentTexture = Textures[i];

                switch (currentTexture.Type)
                {
                    case TextureType.Diffuse:
                        number = DiffuseIdx.ToString();
                        DiffuseIdx++;
                        break;
                    case TextureType.Specular:
                        number = SpecularIdx.ToString();
                        SpecularIdx++;
                        break;
                    case TextureType.Normals:
                        number = NormalIdx.ToString();
                        NormalIdx++;
                        break;
                    case TextureType.Height:
                        number = HeightIdx.ToString();
                        HeightIdx++;
                        break;
                }


                Gl.Uniform1i(Gl.GetUniformLocation(shader.id, $"texture_{currentTexture.Type.ToString().ToLower()}{number}"), 1, i);
                Gl.BindTexture(TextureTarget.Texture2d, Textures[i].Id);
            }

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
