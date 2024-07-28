using ChungusEngine.Graphics;
using ChungusEngine.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.Kinematics
{
    public interface Collider
    {
        public bool TestCollision(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b)
        {
            return a.IsAnyPointBoundBy(b) || b.IsAnyPointBoundBy(a);
        }

        public bool IsInsideOf(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b)
        {
            return a.IsCompletelyBoundBy(b);
        }

        public void OnCollision(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b);

        public AxisAlignedBoundingBox GetBoundingBox();

        public GameObject? GetGameObject()
        {
            return null;
        }
    }

    public static class ColliderExtensions
    {
        public static readonly List<Collider> AllColliders = [];

        public static bool TestCollision(this Collider a, Collider b)
        {
            return a.TestCollision(a.GetBoundingBox(), b.GetBoundingBox());
        }

        public static void RegisterCollider(this Collider a)
        {
            AllColliders.Add(a);
        }
    }
}
