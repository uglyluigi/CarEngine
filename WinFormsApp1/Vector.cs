using System.Runtime.InteropServices;

namespace ChungusEngine.Vector
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2(float x = 0.0f, float y = 0.0f)
    {
        public static Vec2 Zero = new();
        public static Vec2 Unit = new(1.0f, 1.0f);

        public readonly float X => x;
        public readonly float Y => y;

        public readonly float Magnitude { get { return MathF.Sqrt(X * X + Y * Y); } }

        public Vec3 ToVec3()
        {
            return new Vec3(X, Y, 0.0f);
        }

        public float Dot(Vec2 r, float theta)
        {
            return Magnitude * r.Magnitude * MathF.Cos(theta);
        }

        public static Vec2 operator +(Vec2 l, Vec2 r)
        {
            return new(l.X + r.X, l.Y + r.Y);
        }

        public static Vec2 operator -(Vec2 operand)
        {
            return new(-operand.X, -operand.Y);
        }

        public static Vec2 operator -(Vec2 l, Vec2 r)
        {
            return new(l.X - r.X, l.Y - r.Y);
        }

        public static Vec2 operator *(Vec2 l, Vec2 r)
        {
            return new(l.X * r.X, l.Y * r.Y);
        }

        public static Vec2 operator *(float scalar, Vec2 operand)
        {
            return new(scalar * operand.X, scalar * operand.Y);
        }

        public static Vec2 operator /(Vec2 l, Vec2 r)
        {
            return new(l.X / r.X, l.Y / r.Y);
        }

        public readonly override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3(float x = 0.0f, float y = 0.0f, float z = 0.0f)
    {
        public static Vec3 Zero = new();
        public static Vec3 Unit = new(1.0f, 1.0f, 1.0f);

        public readonly float X => x;
        public readonly float Y => y;
        public readonly float Z => z;

        public readonly float Magnitude { get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } }

        public float Dot(Vec3 r, float theta)
        {
            return Magnitude * r.Magnitude * MathF.Cos(theta);
        }

        public static Vec3 operator +(Vec3 l, Vec3 r)
        {
            return new(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        public static Vec3 operator -(Vec3 operand)
        {
            return new(-operand.X, -operand.Y, -operand.Z);
        }

        public static Vec3 operator -(Vec3 l, Vec3 r)
        {
            return new(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        public static Vec3 operator *(Vec3 l, Vec3 r)
        {
            return new(l.X * r.X, l.Y * r.Y, l.Z * r.Z);
        }

        public static Vec3 operator *(float scalar, Vec3 operand)
        {
            return new(scalar * operand.X, scalar * operand.Y, scalar * operand.Z);
        }

        public static Vec3 operator /(Vec3 l, Vec3 r)
        {
            return new(l.X / r.X, l.Y / r.Y, l.Z / r.Z);
        }

        public readonly override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
