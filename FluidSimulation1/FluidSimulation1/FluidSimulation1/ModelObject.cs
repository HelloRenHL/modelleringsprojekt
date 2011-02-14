using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class ModelObject
    {
        public Model Model;
        public Vector3 Position;
        public Vector3 Acceleration;
        public Vector3 Velocity;

        public Vector3 Rotation;

        public Matrix World
        {
            get
            {
                return Matrix.CreateTranslation(Position);
            }
        }

        public ModelObject(Model Model)
        {
            this.Model = Model;
        }

        public void Update()
        {
            this.Velocity += this.Acceleration;
            this.Position += this.Velocity;
        }
    }
}
