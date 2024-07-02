using ChungusEngine.Physics;
using ChungusEngine.Physics.Collision;
using ChungusEngine.src.physics;
using ChungusEngine.UsefulStuff;
using System.Diagnostics;
using System.Numerics;

namespace ChungusEngine.Graphics
{
    public class GameObject : IDisposable, ICollidable
    {
        public readonly Model model;
        private readonly int id;
        public readonly AxisAlignedBoundingBox AABB;

        public GameObject(string objectModelPath, Quaternion rotation, Vector3 position, Vector3 halfExtents)
        {
            model = new Model(objectModelPath, position)
            {
                Rotation = rotation
            };

            id = ModelRegistry.RegisterModel(model);
            model.LoadModel();
            AABB = new(position, halfExtents)
            {
                GameObject = this
            };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ModelRegistry.DeregisterModel(id);
        }

        public void OnCollidedWith(AxisAlignedBoundingBox source)
        {
            Debug.WriteLine("Collision");
        }
    }
}
