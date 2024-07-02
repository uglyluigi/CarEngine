using ChungusEngine.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.src.physics
{
    public interface ICollidable
    {
        public void OnCollidedWith(AxisAlignedBoundingBox source);
    }
}
