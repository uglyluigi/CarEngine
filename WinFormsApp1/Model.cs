﻿using Assimp;
using ChungusEngine.Vector;
using OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using AssMesh = Assimp.Mesh;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Model(string modelPath, Vec3 position)
    {
        public string ModelPath => modelPath;
        public List<Mesh> Meshes { get; private set; } = [];
        public string directory;
        public Vec3 Position => position;
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
                Vec3 vert = new(_vert.X, _vert.Y, _vert.Z);
                var _norm = mesh.Normals[i];
                Vec3 norm = new(_norm.X, _norm.Y, _norm.Z);

                Vec2 uv = Vec2.Zero;

                // get the texture coordinates of this vertex, if present
                if (mesh.HasTextureCoords(i))
                {
                    var currentTextureCoordinate = mesh.TextureCoordinateChannels[0][i];
                    uv = new Vec2(currentTextureCoordinate.X, currentTextureCoordinate.Y);
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

                using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(filename))
                {
                    Gl.BindTexture(TextureTarget.Texture2d, textureId);
                    // fixme probably some problems here because i dont know how to map the
                    // formats available in ImageSharp and OpenGL
                    Rgba32[] texPix = new Rgba32[image.Width * image.Height];
                    image.CopyPixelDataTo(texPix);
                    uint[] transformedData = TransformRgba32ToPackedUintArray(texPix);

                    Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, transformedData);
                    Gl.GenerateMipmap(TextureTarget.Texture2d);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (nint)TexParamValues.GL_REPEAT);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (nint)TexParamValues.GL_REPEAT);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (nint)TexParamValues.GL_LINEAR_MIPMAP_LINEAR);
                    Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (nint)TexParamValues.GL_LINEAR);
                }

                TextureCache.Cache.Add(filename, textureId);
                return textureId;
            }
        }

        private static uint[] TransformRgba32ToPackedUintArray(Rgba32[] texPix)
        {
            var adjustedLength = texPix.Length;
            uint[] transformedData = new uint[adjustedLength];

            for (int i = 0; i < adjustedLength; i++)
            {
                var pixel = texPix[i];

                transformedData[i] = BitConverter.ToUInt32([pixel.R, pixel.G, pixel.B, pixel.A], 0);
            }

            return transformedData;
        }
    }
}
