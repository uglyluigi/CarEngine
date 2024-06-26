using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine
{
    public class GameObject
    {
        private Lazy<Model> model;

        public GameObject(string objectModelPath, Vector3 position)
        {
            model = new(new Model(objectModelPath, position));
            model.LoadModel();
        }
    }
}
