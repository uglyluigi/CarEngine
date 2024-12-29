using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.Graphics
{
    public class PointLight(Vector3 position, Vector3 color)
    {
        public Vector3 Color => color;
        public Vector3 Position => position;
    }
}
