
using OpenGL;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Static
    {
        public static readonly Vector3 Unit3 = new(1.0f, 1.0f, 1.0f);

        public static readonly Vector3 Zero3 = new(0.0f, 0.0f, 0.0f);
        public static readonly Vector2 Zero2 = new(0.0f, 0.0f);

        public static readonly Vector3 XAxis3 = new(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 YAxis3 = new(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 ZAxis3 = new(0.0f, 0.0f, 1.0f);

        public static readonly Vector3 XZPlane3 = new(1.0f, 0.0f, 1.0f);
    }

    public static class Util
    {
        public static readonly Quaternion I = new Quaternion(1.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Quaternion J = new Quaternion(0.0f, 1.0f, 0.0f, 1.0f);
        public static readonly Quaternion K = new Quaternion(0.0f, 0.0f, 1.0f, 1.0f);

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

        public static Matrix4x4f QuatToMatrix2(Quaternion quat)
        {
            var (X, Y, Z, W) = (quat.X, quat.Y, 0, quat.W);

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

        public static Matrix4x4f ToMat4x4fRemoveRoll(this Quaternion quat) => QuatToMatrix2(quat);

        public static Vector3 Normalize(this Vector3 v) => Vector3.Normalize(v);

        public static Vector3 VectorizeQuaternion(Quaternion q) => new Vector3(q.X, q.Y, q.Z);

        public static Vector3 Rotate(Vector3 v, Quaternion q)
        {
            var VectorizedQuaternion = VectorizeQuaternion(q);
            var CrossProduct = Vector3.Cross(VectorizedQuaternion, v);
            return v + 2 * q.W * CrossProduct + 2 * Vector3.Cross(VectorizedQuaternion, CrossProduct);
        }

        public static Vertex3f ToVertex3f(this Vector3 v) => new Vertex3f(v.X, v.Y, v.Z);

        public static (float X, float Y, float Z) Destructure(this Vector3 v) => (v.X, v.Y, v.Z);

        public static (float X, float Y, float Z) Destructure(this Vertex3f v) => (v.x, v.y, v.z);
    }

    public class TextureCache
    {
        public static Dictionary<string, uint> Cache = new();
        public static Dictionary<string, Texture> TexObjCache = new();
    }
}
