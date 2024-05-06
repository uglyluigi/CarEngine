﻿using OpenGL;
using System.Diagnostics;
using System.Text;

namespace ChungusEngine
{
    public class ShaderProgram : IDisposable
    {
        public uint id;
        public int LocationMVP;
        private uint vertex, fragment;

        // Little function for adding newlines at the end of shader source
        // GLSL requires this, otherwise it won't compile
        private static string AddNewline(string s) => s += '\n';

        public ShaderProgram(string vertexShaderSrcPath, string fragmentShaderSecPath)
        {
            string[] vertexShaderSrc = File.ReadAllLines(vertexShaderSrcPath).Select(AddNewline).ToArray();
            string[] fragmentShaderSrc = File.ReadAllLines(fragmentShaderSecPath).Select(AddNewline).ToArray();

            uint vert, frag;

            StringBuilder compilerOutput = new StringBuilder(1024);
            compilerOutput.EnsureCapacity(1024);

            vert = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vert, vertexShaderSrc, null);
            Gl.CompileShader(vert);


            Gl.GetShaderInfoLog(vert, 1024, out _, compilerOutput);

            frag = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(frag, fragmentShaderSrc, null);
            Gl.CompileShader(frag);

            Gl.GetShaderInfoLog(frag, 1024, out _, compilerOutput);

            id = Gl.CreateProgram();
            Gl.AttachShader(id, vert);
            Gl.AttachShader(id, frag);
            Gl.LinkProgram(id);

            Gl.GetProgramInfoLog(id, 1024, out _, compilerOutput);
            vertex = vert;
            fragment = frag;

            if (compilerOutput.Length > 0)
            {
                Debug.WriteLine("--- Shader Compiler Output ---\n" + compilerOutput.ToString());
            }

            LocationMVP = Gl.GetUniformLocation(id, "uMVP");

            if (LocationMVP < 0)
            {
                throw new InvalidOperationException("No uMVP uniform in this shader program");
            }
        }

        public void Use()
        {
            Gl.UseProgram(id);
        }

        public void SetBool(string name, bool value)
        {
            Gl.Uniform1i(Gl.GetUniformLocation(id, name), 1, value);
        }

        public void SetInt(string name, int value)
        {
            Gl.Uniform1i(Gl.GetUniformLocation(id, name), 1, value);
        }

        public void SetFloat(string name, float value)
        {
            Gl.Uniform1i(Gl.GetUniformLocation(id, name), 1, value);
        }

        public void Dispose()
        {
            Gl.DeleteShader(vertex);
            Gl.DeleteShader(fragment);
            Gl.DeleteProgram(id);
        }
    }
}
