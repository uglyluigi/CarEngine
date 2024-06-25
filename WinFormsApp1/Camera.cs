using OpenGL;
using System.Diagnostics;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Camera
    {

        public Quaternion Rotation { get; set; }

        public Vector3 Position = new(0.0f, 0.0f, -5.0f);

        private Vector3 Scale = Static.Unit3;

        private Matrix4x4f CameraTransform { get { 
                return Matrix4x4f.Translated(Position.X, Position.Y, Position.Z) 
                    * Matrix4x4f.Scaled(Scale.X, Scale.Y, Scale.Z) 
                    * Rotation.ToMat4x4fRemoveRoll();
        } }

        public Matrix4x4f View()
        {
            return Matrix4x4f.Identity;
        }

        public Matrix4x4f Projection()
        {
            return Perspective();
        }

        public Vector3 GetForwardVector()
        {
            var inverse = CameraTransform.Inverse;
            return new Vector3(inverse.Column2.x, inverse.Column2.y, inverse.Column2.z).Normalize();
        }

        public Vector3 GetRightVector()
        {
            var v = CameraTransform.Inverse;
            return new Vector3(v.Column0.x, v.Column0.y, v.Column0.z).Normalize();
        }

        static Vertex3f Up = new(0.0f, 1.0f, 0.0f);

        public Vertex3f GetUpVector()
        {
            return Up;
        }

        public Matrix4x4f Perspective()
        {
            return CameraTransform * Matrix4x4f.Perspective(45.0f, 800.0f / 600.0f, 1.0f, 100.0f);
        }

        public void UpdateCameraRotation(Vector2 MouseVector)
        {
            float sensitivity = 0.0005f;
            var XRotation = Quaternion.CreateFromAxisAngle(Static.XAxis3, MouseVector.Y * sensitivity);
            var YRotation = Quaternion.CreateFromAxisAngle(Static.YAxis3, MouseVector.X * sensitivity);
             
            Rotation *= XRotation * YRotation;
        }

        private void MoveForward()
        {
            Position += GetForwardVector() * Static.XZPlane3;
        }

        private void MoveBackward()
        {
            Position -= GetForwardVector() * Static.XZPlane3;
        }

        private void MoveRight()
        {
            Position += GetRightVector() * Static.XZPlane3;
        }

        private void MoveLeft()
        {
            Position -= GetRightVector() * Static.XZPlane3;
        }

        internal void HandleKeyboardInput(KeyEventArgs e)
        {
            var code = e.KeyCode;

            switch (code)
            {
                case Keys.W:
                    MoveForward();
                    break;
                case Keys.A:
                    MoveLeft();
                    break;
                case Keys.S:
                    MoveBackward();
                    break;
                case Keys.D:
                    MoveRight();
                    break;
            }
        }

        internal Camera()
        {
            Rotation = Quaternion.Identity;
        }
    }

}
