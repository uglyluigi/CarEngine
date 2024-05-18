using ChungusEngine.Vector;
using OpenGL;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Camera
    {
        public Quaternion Rotation { get; set; }

        public Vec3 Position { get; set; }

        public Matrix4x4f View()
        {
            return Matrix4x4f.Identity;
        }

        public Matrix4x4f Perspective()
        {
            return Util.QuatToMatrix(Rotation) * Matrix4x4f.Perspective(45.0f, 800.0f / 600.0f, 1.0f, 5.0f);
        }

        public void UpdateCameraRotation((float X, float Y) mouseDelta)
        {
            const float sensitivity = 0.005f;

            // Users are more used to X-axis movements on the mouse corresponding to horizontal rotation
            // and Y-axis movements corresponding to vertical rotation.
            // Originally, the x-Axis quaternion was created wrt. the X-axis and the mouse's change in X.
            // with the Y-axis the same way, except with the mouse's Y-delta.
            // This actually produces an inverted rotation where X movements move the cube up and down (really,
            // down and up) and Y-movements move it left and right (really right and left). So, I swapped them around
            // and now the rotation works as I expect. I should really figure out how these things work
            // to avoid issues later.
            var xAxis = Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), -mouseDelta.Y * sensitivity);
            var yAxis = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), -mouseDelta.X * sensitivity);

            Rotation *= xAxis * yAxis;
        }

        public void UpdateCameraPosition(float dx, float dy, float dz)
        {
            Position += (dx, dy, dz);
        }

        internal Camera()
        {
            Rotation = Quaternion.Identity;
            Position = (0, 0, 0);
        }
    }

}
