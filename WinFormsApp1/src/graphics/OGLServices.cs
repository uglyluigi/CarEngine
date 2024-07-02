using ChungusEngine.UsefulStuff;
using OpenGL;
using StbImageSharp;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static OpenGL.Gl;

namespace ChungusEngine.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        public readonly Vector3 Position => position;
        public readonly Vector3 Normal => normal;
        public readonly Vector2 UV => uv;
    }

    public static class OGLServices
    {
        public static (uint VAO, uint VBO, uint EBO) Buffer(Vertex[] vertices, uint[] indices, bool logBind=false)
        {
            if (logBind)
            {
                foreach (Vertex v in vertices)
                {
                    Debug.Write(v.Position.ToString() + ", ");
                }

                Debug.WriteLine("");
            }
            uint VAO = GenVertexArray();
            uint VBO = GenBuffer();
            uint EBO = GenBuffer();

            Gl.BindVertexArray(VAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            int VERTEX_SIZE = Marshal.SizeOf(typeof(Vertex));


            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(vertices.Length * VERTEX_SIZE), vertices, BufferUsage.StaticDraw);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), indices, BufferUsage.StaticDraw);

            // Expose position vector to shader program
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, VERTEX_SIZE, 0);

            // Expose vertex normal to shader program
            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, VERTEX_SIZE, 12);

            // Expose tex coordinates to shader program
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 2, VertexAttribType.Float, false, VERTEX_SIZE, 24);

            Gl.BindVertexArray(0);

            return (VAO, VBO, EBO);
        }

        public static uint TextureFromFile(string path, string directory)
        {
            string filename = $"{directory}/{path}";

            if (TextureCache.Cache.TryGetValue(filename, out uint value))
            {
                Debug.WriteLine($"Texture cache hit for {filename}");
                return value;
            }
            else
            {
                Debug.WriteLine($"Texture cache miss for {filename}");

                uint textureId = GenTexture();

                using (var stream = File.OpenRead(filename))
                {
                    ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);

                    byte[] data = result.Data;

                    BindTexture(TextureTarget.Texture2d, textureId);
                    TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, result.Width, result.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);
                    GenerateMipmap(TextureTarget.Texture2d);
                    TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, OpenGL.TextureWrapMode.Repeat);
                    TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, OpenGL.TextureWrapMode.Repeat);
                    TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.LinearMipmapLinear);
                    TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
                }

                TextureCache.Cache.Add(filename, textureId);
                return textureId;
            }
        }
    }
}
