
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public class Util
    {
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
    }
}
