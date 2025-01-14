using ChungusEngine.Graphics;
using ChungusEngine.UsefulStuff;
using ChungusEngine.WinForms;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ChungusEngine.src.graphics._2d
{
    public class Sprite2D(string name)
    {
        public SpriteRenderer Renderer = new($"sprites/{name}.png");
    }

    public class SpriteRenderer
    {
        private uint quadVAO;
        private uint SpriteTexture;

        public SpriteRenderer(string texturePath)
        {
            InitRenderData();
            SpriteTexture = OGLServices.TextureFromFile(texturePath);
        }
        
        public void DrawSprite(ShaderProgram program, Vector2 position, Vector2 size, float rotation, Vector3 color)
        {
            Matrix4x4f model = Matrix4x4f.Identity;

            model.Translate(position.X, position.Y, 0.0f);
            model.Translate(0.5f * size.X, 0.5f * size.Y, 0.0f);
            model.RotateZ(rotation);
            model.Translate(-0.5f * size.X, -0.5f * size.Y, 0.0f);
            model.Scale(size.X, size.Y, 1.0f);
            Gl.UniformMatrix4f(program.ModelMatrix, 1, false, model);


            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.Uniform1i(Gl.GetUniformLocation(program.id, "texture_diffuse1"), 1, 1);
            Gl.BindTexture(TextureTarget.Texture2d, SpriteTexture);

            Gl.BindVertexArray(quadVAO);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            Gl.BindVertexArray(0);
        }

        public void InitRenderData()
        {
            // For all 2D sprites, vertices
            // and tex coords will be the same.
            float[] Vertices = [
                0.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 0.0f
            ];

            quadVAO = Gl.GenVertexArray();
            uint VBO = Gl.GenBuffer();

            Gl.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, Convert.ToUInt32(sizeof(float) * Vertices.Length), Vertices, BufferUsage.StaticDraw);

            Gl.BindVertexArray(quadVAO);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 4, VertexAttribType.Float, false, sizeof(float) * 4, 0);

            // Unbind
            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }
    }
}
