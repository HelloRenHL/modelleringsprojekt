using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluidSimulation1
{
    public struct NeighbourPair
    {
        public Particle A;
        public Particle B;

        public NeighbourPair(Particle a, Particle b)
        {
            A = a;
            B = b;
        }
    }

    public class Fluid
    {
        public static Random Random = new Random();
        public int MaxParticles;
        public int ActiveParticles;
        public float Viscosity = 10f;
        public float SurfaceTension = 12.5f;

        public FluidHash FluidHash;

        private int activeParticles;

        List<NeighbourPair> Neighbours = new List<NeighbourPair>();

        public List<Particle> Particles = new List<Particle>();
        float Wpoly6Zero;

        Vector3 Gravity = new Vector3(0, -1.0f, 0) * 196.2f;

        float h;
        float h2;
        float h6;
        float h9;

        AxisAlignedBoundingBox bounds;

        public Fluid(int maxParticles)
        {
            h = 0.15f;
            h2 = h * h;
            h6 = (float)Math.Pow(h, 6);
            h9 = (float)Math.Pow(h, 9);

            bounds = new AxisAlignedBoundingBox();
            bounds.Set(-Vector3.One, Vector3.One);

            FluidHash = new FluidHash(bounds, h);

            MaxParticles = maxParticles;
            activeParticles = maxParticles;

            Wpoly6Zero = Wpoly6(Vector3.Zero);

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new Particle());
            }

            InitializeParticles();
        }

        public void InitializeParticles()
        {
            for (int i = 0; i < activeParticles; i++)
            {
                Particles[i].Position = new Vector3((float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble()) * 2f - Vector3.One;
                Particles[i].Velocity = Vector3.Zero;
                Particles[i].Force = Vector3.Zero;
                Particles[i].Mass = 4f;
                Particles[i].Density = 0f;
                Particles[i].Pressure = 0f;
            }
        }

        private void FindNeighbors()
        {
            this.FluidHash.Clear();
            for (int i = 0; i < this.activeParticles; i++)
            {
                this.FluidHash.AddParticle(this.Particles[i]);
            }

            this.Neighbours.Clear();
            this.FluidHash.GatherNeighbors(Neighbours);
        }

        public float Wpoly6(Vector3 rv)
        {
            return 315 / (64 * MathHelper.Pi * h9) * (float)Math.Pow(h2 - rv.LengthSquared(), 3);
        }

        private void ComputeDensity()
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                Particle a = Neighbours[i].A;
                Particle b = Neighbours[i].B;

                Vector3 distance = a.Position - b.Position;

                float density = a.Mass * this.Wpoly6(distance);

                // Eq. 3 ifrån Müller03
                a.Density += density;
                b.Density += density;
            }
        }

        private void ComputePressure()
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                // Eq. 12 ifrån Müller03
                Particles[i].Pressure = 0.2f * (Particles[i].Density - 1000f);
            }
        }

        private void ComputeAllForces()
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Force += this.Gravity * Particles[i].Mass;
            }

            for (int i = 0; i < Neighbours.Count; i++)
            {
                Particle a = Neighbours[i].A;
                Particle b = Neighbours[i].B;

                float r = (a.Position - b.Position).Length();

                if (r > 0.10f)
                {
                    float r2 = r * r;

                    // Force due to the fluid pressure gradient
                    #region Pressure
                    // Derivative of Eq. 21 Müller03 (Spiky Kernel)
                    Vector3 pressureKernelGradient = 45.0f / (MathHelper.Pi * h6) * ((h2 + r2) / r - 2 * h) * (a.Position - b.Position);

                    //Eq. 10 Müller03
                    Vector3 pressureForce = -b.Mass * (a.Pressure + b.Pressure) / (2 * b.Density) * pressureKernelGradient;
                    #endregion

                    // Force due to the viscosity of the fluid
                    #region Viscosity

                    Vector3 viscosityForce = Vector3.Zero;

                    // This is the gradient of the kernel presented in Eq. 22 Müller03
                    float viscosityKernelLaplacian = 45.0f / (MathHelper.Pi * h6) * (h - r);

                    // Eq. 14 Müller03
                    viscosityForce = Viscosity * b.Mass * (b.Velocity - a.Velocity) / b.Density * viscosityKernelLaplacian; // <- Får allt att hacka

                    #endregion

                    // Müller03 model surface tension forces even thou it is not present in the Navier-Stokes equations (Eq. 7)
                    #region Surface Tension

                    float surfaceTensionKernelLaplacian = 945.0f / (32.0f * MathHelper.Pi * h9) * (h2 - r2) * (7 * r2 - 3 * h2);
                    float colorFieldLaplacian = a.Mass / a.Density * surfaceTensionKernelLaplacian;

                    Vector3 surfaceTensionKernelGradient = 945.0f / (32.0f * MathHelper.Pi * h9) * (h2 - r2) * (h2 - r2) * (a.Position - b.Position);
                    Vector3 n = a.Mass / a.Density * surfaceTensionKernelGradient; // <- The gradient field of the smoothed color field

                    Vector3 surfaceTensionForce = -SurfaceTension * colorFieldLaplacian * n / n.Length();

                    #endregion

                    a.Force += pressureForce + surfaceTensionForce + viscosityForce;
                    b.Force -= pressureForce + surfaceTensionForce + viscosityForce;
                }
            }
        }

        // Funkade skitdåligt?
        /*private void UpdateParticles(float timeStep)
        {
            //the simulation stores position, velocity, velocity half stepped and acceleration for each fluid particle
            for (int i = 0; i < Particles.Count; i++)
            {
                //compute v(t + 1/2dt)
                Vector3 velocityHalfNext = Particles[i].VelocityHalf + timeStep * Particles[i].Force;
                
                //compute r(t + dt)
                Particles[i].Position += timeStep * velocityHalfNext;

                //compute v(t)
                Particles[i].Velocity = 0.5f * (velocityHalfNext + Particles[i].VelocityHalf);
                Particles[i].VelocityHalf = velocityHalfNext;
            }
        }*/

        public void Update(float elapsedTime)
        {
            float timeStep = elapsedTime;

            if (timeStep != 0)
            {
                // Clear particle properties
                for (int i = 0; i < Particles.Count; i++)
                {
                    Particles[i].Force = Vector3.Zero;
                    //Particles[i].Density = 0;
                    Particles[i].Density = Particles[i].Mass * this.Wpoly6Zero;
                    Particles[i].Pressure = 0;
                }

                // Find neighbours of particles
                FindNeighbors();

                // Compute Densities
                ComputeDensity();

                // Compute Pressures
                ComputePressure();

                // Compute All Forces (Including Gravity)
                ComputeAllForces();

                // Update Position and Velocity of each Fluid Particle According to Leap-Frog Scheme
                // UpdateParticles(timeStep);

                for (int i = 0; i < Particles.Count; i++)
                {
                    //simple friction
                    float frictionForce = 0.1f;
                    Particles[i].Force -= Particles[i].Velocity * frictionForce;

                    Particles[i].Velocity += Particles[i].Force / Particles[i].Density * timeStep;
                    Particles[i].Position += Particles[i].Velocity * timeStep;
                    Particles[i].LifeTime += timeStep;
                }

                int size = 1;

                float bounce = 0.33f;

                for (int i = 0; i < Particles.Count; i++)
                {
                    if (Particles[i].Position.Y < 0)
                    {
                        Particles[i].Velocity.Y *= -1 * bounce;
                        Particles[i].Position.Y = 0;
                    }

                    if (Particles[i].Position.X < -size)
                    {
                        Particles[i].Velocity.X *= -1 * bounce;
                        Particles[i].Position.X = -size;
                    }

                    if (Particles[i].Position.X > size)
                    {
                        Particles[i].Velocity.X *= -1 * bounce;
                        Particles[i].Position.X = size;
                    }

                    if (Particles[i].Position.Z > size)
                    {
                        Particles[i].Velocity.Z *= -1 * bounce;
                        Particles[i].Position.Z = size;
                    }

                    if (Particles[i].Position.Z < -size)
                    {
                        Particles[i].Velocity.Z *= -1 * bounce;
                        Particles[i].Position.Z = -size;
                    }
                }
            }
        }
    }
}
