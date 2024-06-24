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

        public Vector3 Position = new();

        public Matrix4x4f View()
        {
            return Matrix4x4f.Identity;
        }

        private (Vector3 Right, Vector3 Up, Vector3 Forward) DeconstructViewMatrix()
        {
            var v = View();

            return (new(v.Column0.x, v.Column0.y, v.Column0.z),
                    new(v.Column1.x, v.Column1.y, v.Column1.z),
                    new(v.Column2.x, v.Column2.y, v.Column2.z)
            );
        }
        public Vector3 GetForwardVector()
        {
            return DeconstructViewMatrix().Forward;
        }

        public Vector3 GetRightVector()
        {
            return DeconstructViewMatrix().Right;
        }

        public Vector3 GetUpVector()
        {
            return new(0, 1, 0);
        }

        public Matrix4x4f Perspective()
        {
            return Matrix4x4f.Translated(Position.X, Position.Y, Position.Z) * Util.QuatToMatrix2(Rotation) * Matrix4x4f.Perspective(45.0f, 800.0f / 600.0f, 1.0f, 100.0f);
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
