using ChungusEngine.Physics;
using ChungusEngine.Physics.Collision;
using ChungusEngine.UsefulStuff;
using System.Diagnostics;
using System.Numerics;
using ChungusEngine.Kinematics;

namespace ChungusEngine.Graphics
{
    public class GameObject : IDisposable, Collider
    {
        public readonly Model model;
        private readonly int id;
        public AxisAlignedBoundingBox AABB;


        public GameObject(string objectModelPath, Quaternion rotation, Vector3 position, Vector3 halfExtents)
        {
            model = new Model(objectModelPath, position)
            {
                Rotation = rotation
            };

            id = ModelRegistry.RegisterModel(model);
            model.LoadModel();
            AABB = new AxisAlignedBoundingBox(position, halfExtents);
            this.RegisterCollider();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ModelRegistry.DeregisterModel(id);
        }

        public AxisAlignedBoundingBox GetBoundingBox()
        {
            return AABB;
        }

        public void OnCollision(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b)
        {

        }

        public GameObject? GetGameObject()
        {
            return this;
        }
    }
}
