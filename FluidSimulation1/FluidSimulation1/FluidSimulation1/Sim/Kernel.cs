using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public static class SmoothKernel
    {
        public static float Poly6(Vector3 rv, float h9, float h2)
        {
            return 315 / (64 * MathHelper.Pi * h9) * (float)Math.Pow(h2 - rv.LengthSquared(), 3);
        }

        public static Vector3 Poly6Gradient(Vector3 rv, float r2, float h2, float h9)
        {
            return 945.0f / (32.0f * MathHelper.Pi * h9) * (float)Math.Pow(h2 - r2, 2) * rv;
        }

        public static float Poly6Laplacian(float r2, float h2, float h9)
        {
            return 945.0f / (32.0f * MathHelper.Pi * h9) * (h2 - r2) * (7 * r2 - 3 * h2);
        }

        // Derivative of Eq. 21 Müller03 (Spiky Kernel)
        public static Vector3 SpikyGradient(Vector3 rv, float r, float r2, float h, float h2, float h6)
        {
            return 45.0f / (MathHelper.Pi * h6) * ((h2 + r2) / r - 2 * h) * rv;
        }

        // This is the laplacian of the kernel presented in Eq. 22 Müller03
        public static float ViscosityLaplacian(float r, float h, float h6)
        {
            return 45.0f / (MathHelper.Pi * h6) * (h - r);
        }
    }
}
