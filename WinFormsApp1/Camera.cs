using OpenGL;
using System.Diagnostics;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Camera
    {
        private Vector3 Acceleration = new();
        private Vector3 Velocity = new();

        public Quaternion Rotation { get; set; }

        public Vector3 Position { get; set; }

        public Matrix4x4f View()
        {
            return Matrix4x4f.Identity;
        }

        public Matrix4x4f Perspective()
        {
            return Matrix4x4f.Translated(Position.X, Position.Y, Position.Z) * Util.QuatToMatrix2(Rotation) * Matrix4x4f.Perspective(45.0f, 800.0f / 600.0f, 1.0f, 50.0f);
        }

        public void UpdateBingusRotation(Vector2 MouseVector)
        {
            Debug.WriteLine(MouseVector);
            float sensitivity = 0.0005f;
            var XRotation = Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MouseVector.Y * sensitivity);
            var YRotation = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MouseVector.X * sensitivity);

            Rotation *= XRotation * YRotation;
        }

        public void UpdateCameraPosition(float dx, float dy, float dz, long dt)
        {
           
        }

        internal void HandleKeyboardInput(KeyEventArgs e, _Direction dir)
        {
            var code = e.KeyCode;
            var moveFactor = 0.01f;

            switch (code)
            {
                case Keys.W:
                    UpdateCameraPosition(moveFactor, 0.0f, 0.0f, 0l);
                    break;
                case Keys.A:
                    UpdateCameraPosition(-moveFactor, 0.0f, 0.0f, 0l);
                    break;
                case Keys.S:
                    UpdateCameraPosition(0.0f, moveFactor, 0.0f, 0l);
                    break;
                case Keys.D:
                    UpdateCameraPosition(0.0f, -moveFactor, 0.0f, 0l);
                    break;
            }
        }

        internal Camera()
        {
            Rotation = Quaternion.Identity;
        }
    }

}
