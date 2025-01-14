using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.src.graphics._2d
{
    public class OrthoCamera
    {
        public Matrix4x4f OthoProjection = Matrix4x4f.Ortho(0, 800.0f, 0.0f, 600.0f, -1.0f, 1.0f);
    }
}
