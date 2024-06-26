using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine
{
    public class GameObject : IDisposable
    {
        private readonly Model model;
        private readonly int id;

        public GameObject(string objectModelPath, Quaternion rotation, Vector3 position)
        {
            model = new Model(objectModelPath, position)
            {
                Rotation = rotation
            };

            id = ModelRegistry.RegisterModel(model);
            model.LoadModel();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ModelRegistry.DeregisterModel(id);
        }
    }
}
