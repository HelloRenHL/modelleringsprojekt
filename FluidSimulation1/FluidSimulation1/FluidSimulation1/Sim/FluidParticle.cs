using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class FluidParticle
    {
        public float Density = 0;
        public float Pressure = 0;
        //public float Mass = 4.0f;
        public float LifeTime = 0;

        public Vector3 Position;
        public Vector3 Velocity = Vector3.Zero;
        public Vector3 VelocityHalf = Vector3.Zero;
        public Vector3 Force = Vector3.Zero;

        public Vector3 Color = Vector3.One;

        public float SurfaceNormal = 0;

        public FluidParticle()
        {

        }
    }
}
