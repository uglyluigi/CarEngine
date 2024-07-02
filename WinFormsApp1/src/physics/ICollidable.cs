using ChungusEngine.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.Physics.Collision
{
    public interface ICollidable
    {
        public void OnCollidedWith(AxisAlignedBoundingBox source);
    }
}
