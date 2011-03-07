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
        public static float h2 = (float)Math.Pow(h, 2);
        public static float h9 = (float)Math.Pow(h, 9);
        public static float h6 = (float)Math.Pow(h, 6);

        public static float h9TimesPi = (float)MathHelper.Pi * h9;
        public static float h6TimesPi = (float)MathHelper.Pi * h6;

        public static float poly6Const = 315.0f / (64.0f * h9TimesPi);
        public static float poly6GradientConst = 945.0f / (32.0f * h9TimesPi);
        public static float poly6LaplacianConst = 945.0f / (32.0f * h9TimesPi);

        public static float spikyGradientConst = 45.0f / (h6TimesPi);
        public static float viscosityLaplacianConst = 45.0f / (h6TimesPi);

        public static float Poly6(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 && r <= h)
            {
                return poly6Const * (float)Math.Pow(h2 - r * r, 3);
            }

            return 0.0f;
        }

        public static Vector3 Poly6Gradient(Vector3 rv)
        {
            float r = rv.Length();
            if (r >= 0 && r <= h)
            {
                return poly6GradientConst * (float)Math.Pow(h2 - r * r, 2) * rv;
            }

            return Vector3.Zero;
        }

        public static float Poly6Laplacian(Vector3 rv)
        {
            float r = rv.Length();
            float r2 = (float)Math.Pow(r, 2);
            if (r >= 0 & r <= h)
            {
                return poly6LaplacianConst * (h2 - r2) * (7 * r2 - 3 * h2);
            }

            return 0.0f;
        }

        // Derivative of Eq. 21 Müller03 (Spiky Kernel)
        public static Vector3 SpikyGradient(Vector3 rv)
        {
            float r = rv.Length();

            if (r > 0 && r <= h) //r > 0 <-- r får absolut inte vara 0! då krashar det! :P
            {
                return spikyGradientConst * ((h2 + r * r) / r - 2 * h) * rv;
            }

            return Vector3.Zero;
        }

        // This is the laplacian of the kernel presented in Eq. 22 Müller03
        public static float ViscosityLaplacian(Vector3 rv)
        {
            float r = rv.Length();

            if (r >= 0 && r <= h)
            {
                return viscosityLaplacianConst * (h - r);
            }

            return 0.0f;
        }
    }
}
