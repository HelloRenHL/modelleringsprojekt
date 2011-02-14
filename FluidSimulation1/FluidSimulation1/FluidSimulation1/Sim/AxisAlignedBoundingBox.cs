using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class AxisAlignedBoundingBox
    {
        public Vector3 Max;
        public Vector3 Min;

        public bool Inside(ref Vector3 p)
        {
            if (((p.X < Min.X) ||
                (p.X > Max.X)) ||
                ((p.Y < Min.Y) ||
                (p.Y > Max.Y)))
            {
                return false;
            }

            return (p.Z >= Min.Z) && (p.Z <= Max.Z);
        }

        public void Set(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
    }
}
