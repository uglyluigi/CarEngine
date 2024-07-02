using Assimp;
using ChungusEngine.UsefulStuff;
using OpenGL;
using StbImageSharp;
using System.Diagnostics;
using System.Numerics;
using AssMesh = Assimp.Mesh;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine.Graphics
{
    public class Model
    {
        public string ModelPath;
        public List<Mesh> Meshes { get; private set; } = [];
        public string directory;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.Identity;
        public Matrix4x4f ModelTransform { get { return Util.QuatToMatrix(Rotation) * Matrix4x4f.Translated(Position.X, Position.Y, Position.Z); } }

        public Model(string modelPath, Vector3 position)
        {
            Position = position;
            ModelPath = modelPath;
        }


        public void Draw(ShaderProgram program)
        {
            foreach (var mesh in Meshes)
            {
                Gl.UniformMatrix4f(program.ModelMatrix, 1, false, ModelTransform);

                mesh.Draw(program);
            }
        }

        public void LoadModel(PostProcessSteps steps =
            PostProcessSteps.Triangulate |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.GenerateSmoothNormals |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.OptimizeMeshes
        )
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
                // FIXME some models use the other UV channels
                if (mesh.HasTextureCoords(0))
                {
                    var currentTextureCoordinate = mesh.TextureCoordinateChannels[0][i];
                    uv = new(currentTextureCoordinate.X, currentTextureCoordinate.Y);
                }

                vertices.Add(new(vert, norm, uv));
            }

            foreach (var face in mesh.Faces)
            {
                foreach (var index in face.Indices)
                {
                    indices.Add(Convert.ToUInt32(index));
                }
            }

            var material = scene.Materials[mesh.MaterialIndex];
            var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse);
            var specularMaps = LoadMaterialTextures(material, TextureType.Specular);
            var normalMaps = LoadMaterialTextures(material, TextureType.Normals);
            var heightMaps = LoadMaterialTextures(material, TextureType.Ambient);

            textures = [.. textures, .. diffuseMaps, .. specularMaps, .. normalMaps, .. heightMaps];


            return new Mesh(vertices, indices, textures);
        }

        private List<Texture> LoadMaterialTextures(Material material, TextureType type)
        {
            List<Texture> textures = [];

            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                TextureSlot slot;
                material.GetMaterialTexture(type, i, out slot);

                Texture textureToAdd;

                if (TextureCache.TexObjCache.TryGetValue(slot.FilePath, out Texture t))
                {
                    textureToAdd = t;
                }
                else
                {
                    Texture tex = new(OGLServices.TextureFromFile(slot.FilePath, directory), type, slot.FilePath);
                    TextureCache.TexObjCache.Add(slot.FilePath, tex);
                    textureToAdd = tex;
                }

                textures.Add(textureToAdd);
            }

            return textures;
        }
    }
}
