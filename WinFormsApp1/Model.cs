using Assimp;
using OpenGL;
using StbImageSharp;
using System.Diagnostics;
using System.Numerics;
using AssMesh = Assimp.Mesh;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Model(string modelPath, Vector3 position)
    {
        public string ModelPath => modelPath;
        public List<Mesh> Meshes { get; private set; } = [];
        public string directory;
        public Vector3 Position => position;
        public Quaternion Rotation = Quaternion.Identity;
        public Matrix4x4f ModelTransform { get { return Matrix4x4f.Identity * Util.QuatToMatrix(Rotation) * Matrix4x4f.Translated(Position.X, Position.Y, Position.Z); } }


        public void Draw(ShaderProgram program)
        {
            foreach (var mesh in Meshes)
            {
                Gl.UniformMatrix4f(program.ModelMatrix, 1, false, ModelTransform);

                mesh.Draw(program);
            }
        }

        public void LoadModel(PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs)
        {
            AssimpContext ctx = new AssimpContext();
            // info on these bit flags
            // https://learnopengl.com/Model-Loading/Model

            Scene s;

            try
            {
                s = ctx.ImportFile(ModelPath, steps);
            }
            catch (Exception e) when (e is AssimpException or ObjectDisposedException)
            {
                Console.WriteLine("Unknown error when loading model!");
                throw;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Failed to load file. File not found: {ModelPath})");
                throw;
            }


            directory = ModelPath[..ModelPath.LastIndexOf('/')];
            ProcessNode(s.RootNode, s);
        }

        private void ProcessNode(Node node, Scene scene)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                var mesh = scene.Meshes[node.MeshIndices[i]];
                Meshes.Add(ProcessMesh(mesh, scene));
            }

            foreach (var child in node.Children)
            {
                ProcessNode(child, scene);
            }

        }

        private Mesh ProcessMesh(AssMesh mesh, Scene scene)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];
            List<Texture> textures = [];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var _vert = mesh.Vertices[i];
                Vector3 vert = new(_vert.X, _vert.Y, _vert.Z);
                var _norm = mesh.Normals[i];
                Vector3 norm = new(_norm.X, _norm.Y, _norm.Z);

                Vector2 uv = Static.Zero2;

                // get the texture coordinates of this vertex, if present
                if (mesh.HasTextureCoords(i))
                {
                    var currentTextureCoordinate = mesh.TextureCoordinateChannels[0][i];
                    uv = new Vector2(currentTextureCoordinate.X, currentTextureCoordinate.Y);
                }

                vertices.Add(new Vertex(vert, norm, uv));
            }

            foreach (var face in mesh.Faces)
            {
                foreach (var index in face.Indices)
                {
                    indices.Add(Convert.ToUInt32(index));
                }
            }

            if (mesh.MaterialIndex >= 0)
            {
                var material = scene.Materials[mesh.MaterialIndex];
                var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse);
                textures.AddRange(diffuseMaps);
                var specularMaps = LoadMaterialTextures(material, TextureType.Specular);
                textures.AddRange(specularMaps);
            }

            return new Mesh(vertices, indices, textures);
        }

        private List<Texture> LoadMaterialTextures(Material material, TextureType type)
        {
            List<Texture> textures = [];

            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                TextureSlot slot;
                material.GetMaterialTexture(type, i, out slot);
                textures.Add(new Texture(TextureFromFile(slot.FilePath, directory), slot.TextureType, slot.FilePath));
            }

            return textures;
        }


        uint TextureFromFile(string path, string directory)
        {
            string filename = $"{directory}/{path}";

            if (TextureCache.Cache.TryGetValue(filename, out uint value))
            {
                Debug.WriteLine($"Texture cache hit for {filename}");
                return value;
            } else
            {
                Debug.WriteLine($"Texture cache miss for {filename}");

                uint textureId = Gl.GenTexture();

                using (var stream = File.OpenRead(filename))
                {
                    ImageResult result = ImageResult.FromStream(stream);
                    byte[] data = result.Data;

                    Gl.BindTexture(TextureTarget.Texture2d, textureId);
                    Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, result.Width, result.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);
                    Gl.GenerateMipmap(TextureTarget.Texture2d);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, OpenGL.TextureWrapMode.Repeat);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, OpenGL.TextureWrapMode.Repeat);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.LinearMipmapLinear);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
                }

                TextureCache.Cache.Add(filename, textureId);
                return textureId;
            }
        }
    }
}
