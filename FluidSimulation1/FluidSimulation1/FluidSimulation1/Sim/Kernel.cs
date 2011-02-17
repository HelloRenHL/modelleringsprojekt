using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public static class SmoothKernel
    {
        public static float h = 0.15f;

        public static float Poly6(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 && r <= h)
            {
                float h9 = (float)Math.Pow(h, 9);
                return 315.0f / (64.0f * MathHelper.Pi * h9) * (float)Math.Pow(h * h - r * r, 3);
            }

            return 0.0f;
        }

        public static Vector3 Poly6Gradient(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 && r <= h)
            {
                float h9 = (float)Math.Pow(h, 9);
                return 945.0f / (32.0f * MathHelper.Pi * h9) * (float)Math.Pow(h * h - r * r, 2) * rv;
            }

            return Vector3.Zero;
        }

        public static float Poly6Laplacian(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 & r <= h)
            {
                float h9 = (float)Math.Pow(h, 9);
                return 945.0f / (32.0f * MathHelper.Pi * h9) * (h * h - r * r) * (7 * r * r - 3 * h * h);
            }

            return 0.0f;
        }

        // Derivative of Eq. 21 Müller03 (Spiky Kernel)
        public static Vector3 SpikyGradient(Vector3 rv)
        {
            float r = rv.Length();

            if (r > 0 && r <= h) //r > 0 <-- r får absolut inte vara 0! då krashar det! :P
            {
                float h6 = (float)Math.Pow(h, 6);
                return 45.0f / (MathHelper.Pi * h6) * ((h * h + r * r) / r - 2 * h) * rv;
            }

            return Vector3.Zero;
        }

        // This is the laplacian of the kernel presented in Eq. 22 Müller03
        public static float ViscosityLaplacian(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 && r <= h)
            {
                float h6 = (float)Math.Pow(h, 6);
                return 45.0f / (MathHelper.Pi * h6) * (h - r);
            }

            return 0.0f;
        }
    }
}
