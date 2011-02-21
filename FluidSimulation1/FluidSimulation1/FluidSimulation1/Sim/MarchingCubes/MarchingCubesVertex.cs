using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class MarchingCubesVertex
    {
        public Vector3 Normal;
        public int NumTrisConnected;
        public Vector3 Position;
        public Vector3 Temp;
    }
}
