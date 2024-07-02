using ChungusEngine.Graphics;
using System.Diagnostics;

namespace ChungusEngine.Physics.Collision
{
    public static class CollisionTest
    {
        public static void RunCollisionTests(List<GameObject> gameObjects)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (WindowProvider.Camera.BoundingBox.IsAnyPointBoundBy(obj.AABB))
                {
                    Debug.WriteLine("Collision");
                }
            }
        }
    }
}
