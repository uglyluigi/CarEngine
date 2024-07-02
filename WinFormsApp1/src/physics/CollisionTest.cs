using ChungusEngine.Graphics;

namespace ChungusEngine.Physics.Collision
{
    public static class CollisionTest
    {
        public static void RunCollisionTests(List<GameObject> gameObjects)
        {
            foreach (var obj1 in gameObjects)
            {
                foreach (var obj2 in gameObjects)
                {
                    if (obj1 == obj2) continue;

                    foreach (var bound in obj1.AABB.Bounds())
                    {
                        if (obj2.AABB.IsPointBound(bound))
                        {
                            obj1.OnCollidedWith(obj2.AABB);
                        }
                    }

                    foreach (var bound in obj2.AABB.Bounds())
                    {
                        if (obj1.AABB.IsPointBound(bound))
                        {
                            obj2.OnCollidedWith(obj1.AABB);
                        }
                    }
                }
            }
        }
    }
}
