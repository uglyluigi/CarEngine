using ChungusEngine.Vector;
using OpenGL;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Camera
    {
        public Matrix4x4f View()
        {
            return Matrix4x4f.LookAt(new(Position.X, Position.Y, Position.Z), new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f, 1.0f));
        }

        public Matrix4x4f Perspective()
        {
            return Matrix4x4f.Perspective(45.0f, 800.0f / 600.0f, 1.0f, 5.0f);
        }

        public Matrix4x4f Ortho()
        {
            return Matrix4x4f.Ortho(0.0f, 869.0f, 0, 533.0f, 0.0f, 100.0f);
        }

        public Matrix4x4f Model()
        {
            return Matrix4x4f.Identity;
        }

        public Quaternion Rotation { get; set; }

        private Matrix4x4f RotMatrix { get { return QuatToMatrix(Rotation); } }

        public Matrix4x4f MVP { get { return Perspective() * View() * RotMatrix * Model(); } }

        public Matrix4x4f RotateMat { get; set; }

        public Matrix4x4f MVP_DEBUG { get { return RotMatrix * Matrix4x4f.Translated(0.0f, 0.0f, -3.0f) * Perspective(); } }

        public Vec3 Position { get; set; }

        // potential FIXME: may require normalization
        /**
         * This is literally magic. The product of these two
         * matrices produces a rotation matrix you can apply
         * to your view matrix to rotate the camera.
         */
        public static Matrix4x4f QuatToMatrix(Quaternion quat)
        {
            var (X, Y, Z, W) = (quat.X, quat.Y, quat.Z, quat.W);

            return
                new Matrix4x4f(
                    W, Z, -Y, X, 
                    -Z, W, X, Y,
                    Y, -X, W, Z,
                    -X, -Y, -Z, W
                )
                *
                new Matrix4x4f(
                    W, Z, -Y, -X,
                    -Z, W, X, -Y,
                    Y, -X, W, -Z, 
                    X, Y, Z, W
                );
        }


        internal Camera()
        {
            Rotation = Quaternion.Identity;
            Position = (0, 0, 0);
        }
    }

}
