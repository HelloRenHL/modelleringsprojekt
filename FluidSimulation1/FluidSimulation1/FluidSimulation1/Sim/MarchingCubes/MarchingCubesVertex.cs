using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class MarchingCubesVertex
    {
        public float b;
        public Vector3 normal;
        public int numTrisConnected;
        public Vector3 position;
        public Vector3 temp;
    }
}
