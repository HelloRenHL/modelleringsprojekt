using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class Particle
    {
        public float Density = 0;
        public Vector3 Position;

        public float Mass = 1.0f;
        public float Pressure = 0;

        public Vector3 Velocity = Vector3.Zero;
        public Vector3 VelocityHalf = Vector3.Zero;

        public Vector3 Force = Vector3.Zero;

        public float LifeTime = 0;

        public Particle()
        {

        }
    }
}
