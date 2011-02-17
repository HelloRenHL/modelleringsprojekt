using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class SmoothKernel
    {

        public const float Poly6Constant = 315.0f / (64.0f * MathHelper.Pi);
        public const float Poly6GradientConstant = 945.0f / (32.0f * MathHelper.Pi);
        public const float SpikyGradientConstant = 45.0f / MathHelper.Pi;

        public static float h = 0.15f;
        public static float h2 = 0.0225f;
        public static float h4 = 0.00050625f;
        public static float h6Inv = 87791.4952f;
        public static float h9Inv = 26012294.9f;

        public static float Poly6(float r)
        {
            if (r > h)
            {
                return 0;
            }

            return Poly6Constant * h9Inv * (float)Math.Pow(h * h - r * r, 3);
        }

        public static Vector3 Poly6Gradient(Vector3 rv)
        {
            float r = rv.Length();

            if (r > h)
            {
                return Vector3.Zero;
            }

            float r2 = rv.LengthSquared();
            float r4 = r2 * r2;

            return Poly6GradientConstant * h9Inv * (h4 - 2 * h2 * r2 + r4) * rv;
        }

        public static float Poly6Laplacian(Vector3 rv)
        {
            float r = rv.Length();

            if (r > h)
            {
                return 0;
            }

            float r2 = rv.LengthSquared();
            return h9Inv * (h2 - r2) * (7 * r2 - 3 * h2);
        }

        // Derivative of Eq. 21 Müller03 (Spiky Kernel)
        public static Vector3 SpikyGradient(Vector3 rv)
        {
            float r = rv.Length();

            if (r > h)
            {
                return Vector3.Zero;
            }

            float r2 = rv.LengthSquared();

            return SpikyGradientConstant * h6Inv * ((h2 + r2) / r - 2 * h) * rv;
        }

        // This is the laplacian of the kernel presented in Eq. 22 Müller03
        public static float ViscosityLaplacian(Vector3 rv)
        {
            float r = rv.Length();

            if (r > h)
            {
                return 0;
            }

            return SpikyGradientConstant * h6Inv * (h - r);
        }
    }
}
