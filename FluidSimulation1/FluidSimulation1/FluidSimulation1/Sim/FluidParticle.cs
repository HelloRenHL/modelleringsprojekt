using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class FluidParticle
    {
        public int Mass;
        public float Density;

        public Vector3 Force;
        public Vector3 Velocity;
        public Vector3 Position;

        public TimeSpan LifeSpan;
        public float Pressure;
    }
}
