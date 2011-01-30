using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluidSimulation1
{
    public struct NeighbourPair
    {
        public FluidParticle A;
        public FluidParticle B;

        public NeighbourPair(FluidParticle a, FluidParticle b)
        {
            A = a;
            B = b;
        }
    }

    public class Fluid
    {
        public static Random Random = new Random();
        public int MaxParticles;
        public float Viscosity = 10f;
        public float SurfaceTension = 12.5f;

        public FluidHash FluidHash;

        public int ActiveParticles;

        List<NeighbourPair> Neighbours = new List<NeighbourPair>();

        public List<FluidParticle> Particles = new List<FluidParticle>();
        float Poly6Zero;

        public Vector3 Gravity = Vector3.Down * 196.2f;
        public float GravityRotation = 0.0f;

        float h;

        AxisAlignedBoundingBox bounds;

        public Fluid(int maxParticles)
        {
            h = 0.15f;

            bounds = new AxisAlignedBoundingBox();
            bounds.Set(-Vector3.One, Vector3.One);

            FluidHash = new FluidHash(bounds, h);

            MaxParticles = maxParticles;
            ActiveParticles = maxParticles;

            Poly6Zero = SmoothKernel.Poly6(Vector3.Zero, h);

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new FluidParticle());
            }

            InitializeParticles();
        }

        public void InitializeParticles()
        {
            for (int i = 0; i < ActiveParticles; i++)
            {
                Particles[i].Position = new Vector3((float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble()) * 2f - Vector3.One;
                Particles[i].Velocity = Vector3.Zero;
                Particles[i].Force = Vector3.Zero;
                Particles[i].Mass = 4f;
                Particles[i].Density = 0f;
                Particles[i].Pressure = 0f;
            }
        }

        public Stopwatch StopWatch = new Stopwatch();

        private void FindNeighbors()
        {
            this.FluidHash.Clear();
            for (int i = 0; i < this.ActiveParticles; i++)
            {
                this.FluidHash.AddParticle(this.Particles[i]);
            }

            this.Neighbours.Clear();
            this.FluidHash.GatherNeighbors(Neighbours);
        }

        private void ComputeDensity()
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                FluidParticle a = Neighbours[i].A;
                FluidParticle b = Neighbours[i].B;

                float density = a.Mass * SmoothKernel.Poly6(b.Position - a.Position, h);

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
                Particles[i].Pressure = 0.2f * (Particles[i].Density - 1000f); //<- ???
            }
        }

        private void ComputeAllForces()
        {

            Vector3 tempGravity = Vector3.Transform(Gravity, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(GravityRotation)));

            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Force += tempGravity * Particles[i].Mass;
            }

            for (int i = 0; i < Neighbours.Count; i++)
            {
                FluidParticle a = Neighbours[i].A;
                FluidParticle b = Neighbours[i].B;

                Vector3 rv = a.Position - b.Position;
                float r = rv.Length();
                float r2 = r * r;

                // Force due to the fluid pressure gradient
                #region Pressure

                //Eq. 10 Müller03
                Vector3 pressureForce = -b.Mass / b.Density * (a.Pressure + b.Pressure) / 2.0f * -SmoothKernel.SpikyGradient(rv, h); //La in ett minus här (-Smoothed...)

                #endregion

                // Force due to the viscosity of the fluid
                #region Viscosity

                Vector3 viscosityForce = Vector3.Zero;

                // Eq. 14 Müller03
                viscosityForce = Viscosity * b.Mass * (b.Velocity - a.Velocity) / b.Density * SmoothKernel.ViscosityLaplacian(rv, h);

                #endregion

                // Müller03 model surface tension forces even thou it is not present in the Navier-Stokes equations (Eq. 7)
                #region Surface Tension

                Vector3 surfaceTensionForce = Vector3.Zero;

                // Müller03: "Evaluating n/|n| at locations where |n| is small causes numerical problems. We only evaluate the force if |n| exceeds a certain threshold
                float threshold = 0.05f;

                float colorFieldLaplacian = a.Mass / a.Density * SmoothKernel.Poly6Laplacian(rv, h);
                Vector3 n = a.Mass / a.Density * SmoothKernel.Poly6Gradient(rv, h); // <- The gradient field of the smoothed color field
                
                if (n.Length() > threshold)
                {
                    surfaceTensionForce = -SurfaceTension * colorFieldLaplacian * n / n.Length();
                }

                #endregion

                a.Force += surfaceTensionForce + viscosityForce + pressureForce;
                b.Force -= (surfaceTensionForce + viscosityForce + pressureForce);
            }
        }

        /// <summary>
        /// Update position according to Leap-Frog scheme (This is not being used)
        /// </summary>
        /// <param name="timeStep"></param>
        private void UpdateParticles(float timeStep)
        {
            //the simulation stores position, velocity, velocity half stepped and acceleration for each fluid particle
            for (int i = 0; i < Particles.Count; i++)
            {
                //compute v(t + 1/2dt)
                Vector3 velocityHalfNext = Particles[i].VelocityHalf + timeStep * (Particles[i].Force / Particles[i].Density);

                //compute r(t + dt)
                Particles[i].Position += timeStep * velocityHalfNext;

                //compute v(t)
                Particles[i].Velocity = 0.5f * (velocityHalfNext + Particles[i].VelocityHalf);
                Particles[i].VelocityHalf = velocityHalfNext;
            }
        }

        public void Update(float elapsedTime)
        {
            float timeStep = elapsedTime;

            StopWatch.Reset();
            StopWatch.Start();

            if (timeStep != 0)
            {
                // Clear particle properties
                for (int i = 0; i < Particles.Count; i++)
                {
                    Particles[i].Force = Vector3.Zero;
                    //Particles[i].Density = 0;
                    Particles[i].Density = Particles[i].Mass * this.Poly6Zero;
                    Particles[i].Pressure = 0;
                }

                // Find neighbours of particles
                FindNeighbors();

                // Litar inte på att funktionen ovanför funkar till 100% så skrev en egen för att testa
                //TempFindNeighbors();

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

                HandleCollisions();
            }

            StopWatch.Stop();
        }

        private void TempFindNeighbors()
        {
            Neighbours.Clear();
            for (int i = 0; i < Particles.Count; i++)
            {
                for (int j = 0; j < Particles.Count; j++)
                {
                    if (i == j)
                        continue;

                    float r = (Particles[i].Position - Particles[j].Position).Length();

                    if (r < h)
                    {
                        Neighbours.Add(new NeighbourPair(Particles[i], Particles[j]));
                    }
                }
            }
        }

        private void HandleCollisions()
        {
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
