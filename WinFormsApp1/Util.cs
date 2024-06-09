﻿
using OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Static
    {
        public static readonly Vector3 Zero3 = new(0.0f, 0.0f, 0.0f);
        public static readonly Vector2 Zero2 = new(0.0f, 0.0f);
    }

    public class Util
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

        internal static Quaternion RemoveRoll(Quaternion rotation)
        {
            float Pitch = rotation.Y;
            var CancelPitch = new Quaternion(0.0f, Pitch, 0.0f, 0.0f);

            float Yaw = rotation.X;
            var CancelYaw = new Quaternion(Yaw, 0.0f, 0.0f, 0.0f);
            
            return Quaternion.Normalize(CancelPitch) * Quaternion.Normalize(CancelYaw);
        }
    }

    public class TextureCache
    {
        public static Dictionary<string, uint> Cache = new();
    }
}
