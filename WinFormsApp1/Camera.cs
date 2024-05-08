using ChungusEngine.Vector;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine
{
    public class Camera
    {
        public Matrix4x4f ModelView { get; set; }
        public Matrix4x4f Projection { get { return Matrix4x4f.Perspective(90.0f, 16.0f / 9.0f, 0.1f, 100.0f) * Matrix4x4f.Translated(Position.X, Position.Y, Position.Z); } set { ModelView = value; } }

        public Matrix4x4f MVP { get { return Projection * ModelView; } }

        public Vec3 Position { get; set; }

        internal Camera()
        {
            ModelView = Matrix4x4f.Identity;
        }
    }

}
