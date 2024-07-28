using ChungusEngine.Graphics;
using OpenGL;
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
                   
                }
                else
                {
                    Gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
                }
            }
        }
    }
}
