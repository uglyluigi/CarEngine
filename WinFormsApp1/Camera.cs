using ChungusEngine.Vector;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Camera
    {
        public Matrix4x4f View()
        {
            return Matrix4x4f.LookAt(new(Position.X, Position.Y, Position.Z), new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f, 1.0f));
        }

        public Matrix4x4f Projection()
        {
            return Matrix4x4f.Perspective(90.0f, 800.0f / 600.0f, 1.0f, 100.0f);
        }

        public Matrix4x4f Model()
        {
            return Matrix4x4f.Identity;
        }

        public System.Numerics.Quaternion Rotation { get; set; }

        private Matrix4x4f RotMatrix { get { return QuatToMatrix(Rotation); } }

        public Matrix4x4f MVP { get { return Projection() * View() * RotMatrix * Model(); } }

        public Vec3 Position { get; set; }

        // potential FIXME: may require normalization
        public static Matrix4x4f QuatToMatrix(Quaternion quat)
        {
            return
                new Matrix4x4f(
                    quat.W, quat.Z, -quat.Y, quat.X, 
                    -quat.Z, quat.W, quat.X, quat.Y,
                    quat.Y, -quat.X, quat.W, quat.Z,
                    -quat.X, -quat.Y, -quat.Z, quat.W
                )
                *
                new Matrix4x4f(
                    quat.W, quat.Z, -quat.Y, -quat.X,
                    -quat.Z, quat.W, quat.X, -quat.Y,
                    quat.Y, -quat.X, quat.W, -quat.Z,
                    quat.X, quat.Y, quat.Z, quat.W
                );
        }


        internal Camera()
        {
            Rotation = Quaternion.Identity;
        }
    }

}
