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
        public int MaxParticles;
        public int ActiveParticles;

        public float Viscosity = 10f;
        public float SurfaceTension = 12.5f;
        public float ParticleMass = 4.0f;

        public FluidHash FluidHash;

        List<NeighbourPair> Neighbours = new List<NeighbourPair>();

        public List<FluidParticle> Particles = new List<FluidParticle>();
        float Poly6Zero;

        public Vector3 Gravity = Vector3.Down * 196.2f;
        public float GravityRotation = 0.0f;

        public AxisAlignedBoundingBox Bounds;

        public Fluid(int maxParticles)
        {
            Bounds = new AxisAlignedBoundingBox();
            //bounds.Set(-Vector3.One, Vector3.One);
            int size = 1;

            //bounds.Set(new Vector3(-1f, -1f, -1f) * size, new Vector3(1f, 1f, 1f) * size);
            
            Bounds.Set(new Vector3(-1f, -0.5f, -0.2f) * size, new Vector3(1f, 0.5f, 0.2f) * size);
            FluidHash = new FluidHash(Bounds, SmoothKernel.h);

            //bounds.Set(new Vector3(-1f, -0.5f, -0.2f) * size, new Vector3(1f, 0.5f, 0.2f) * size);
            //bounds.Max = Vector3.Transform(bounds.Max, Matrix.CreateRotationY(MathHelper.ToRadians(30)));
            //bounds.Min = Vector3.Transform(bounds.Min, Matrix.CreateRotationY(MathHelper.ToRadians(30)));
            
            MaxParticles = maxParticles;
            ActiveParticles = maxParticles;

            Poly6Zero = SmoothKernel.Poly6(Vector3.Zero);

            InitializeParticles();
        }

        public void CreateColorField(ref float[] inside, float minParticleSize, float bx, float by, float bz, float dx, float dy, float dz, int MCX, int MCY, int MCZ)
        {
            for (int n = 0; n < ((MCX * MCY) * MCZ); n++)
            {
                inside[n] = minParticleSize;
            }

            for (int n = 0; n < this.ActiveParticles; n++)
            {
                FluidParticle particle = this.Particles[n];

                Vector3 position = particle.Position;

                int num5 = (int)(((position.X - 0.15f) - bx) / dx);
                int num6 = (int)(((position.Y - 0.15f) - by) / dy);
                int num7 = (int)(((position.Z - 0.15f) - bz) / dz);
                int num8 = (int)(((position.X + 0.15f) - bx) / dx);
                int num9 = (int)(((position.Y + 0.15f) - by) / dy);
                int num10 = (int)(((position.Z + 0.15f) - bz) / dz);

                if (num5 < 0)
                {
                    num5 = 0;
                }
                else if (num5 >= MCX)
                {
                    num5 = MCX - 1;
                }
                if (num6 < 0)
                {
                    num6 = 0;
                }
                else if (num6 >= MCY)
                {
                    num6 = MCY - 1;
                }
                if (num7 < 0)
                {
                    num7 = 0;
                }
                else if (num7 >= MCZ)
                {
                    num7 = MCZ - 1;
                }
                if (num8 < 0)
                {
                    num8 = 0;
                }
                else if (num8 >= MCX)
                {
                    num8 = MCX - 1;
                }
                if (num9 < 0)
                {
                    num9 = 0;
                }
                else if (num9 >= MCY)
                {
                    num9 = MCY - 1;
                }
                if (num10 < 0)
                {
                    num10 = 0;
                }
                else if (num10 >= MCZ)
                {
                    num10 = MCZ - 1;
                }
                for (int i = num7; i <= num10; i++)
                {
                    for (int j = num6; j <= num9; j++)
                    {
                        for (int k = num5; k <= num8; k++)
                        {
                            Vector3 p = new Vector3(((k + 0.5f) * dx) + bx, ((j + 0.5f) * dy) + by, ((i + 0.5f) * dz) + bz);
                            if (this.Bounds.Inside(ref p))
                            {
                                Vector3 rv = position - p;
                                if (rv.LengthSquared() < 0.0225f)
                                {
                                    inside[(((i * MCY) + j) * MCX) + k] -= SmoothKernel.Poly6(rv) * particle.SurfaceNormal;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector3 Random(Vector3 min, Vector3 max)
        {
            Vector3 vector = new Vector3((float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble());
            Vector3 vector2 = max - min;
            return (min + (vector2 * vector));
        }

        public void InitializeParticles()
        {
            Particles.Clear();
            for (int i = 0; i < MaxParticles; i++)
            {
                FluidParticle particle = new FluidParticle();
                particle.Position = Random(Bounds.Min, Bounds.Max);
                particle.Velocity = Vector3.Zero;
                particle.Force = Vector3.Zero;
                particle.Color = new Vector3((float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble(), (float)Game1.Random.NextDouble());
                //particle.Mass = ParticleMass;
                particle.Density = 0f;
                particle.Pressure = 0f;
                Particles.Add(particle);
            }
        }

        public Stopwatch StopWatch = new Stopwatch();

        private void FindNeighbors()
        {
            this.FluidHash.Clear();
            for (int i = 0; i < ActiveParticles; i++)
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

                float density = ParticleMass * SmoothKernel.Poly6(b.Position - a.Position);

                // Eq. 3 ifrån Müller03
                a.Density += density;
                b.Density += density;
            }
        }

        private void ComputePressure()
        {
            for (int i = 0; i < ActiveParticles; i++)
            {
                Particles[i].SurfaceNormal = 1.0f / Particles[i].Density * ParticleMass;

                // Eq. 12 ifrån Müller03
                Particles[i].Pressure = 0.2f * (Particles[i].Density - 1000f); //<- ???
            }
        }

        private void ComputeAllForces()
        {
            Vector3 tempGravity = Vector3.Transform(Gravity, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(GravityRotation)));

            for (int i = 0; i < ActiveParticles; i++)
            {
                Particles[i].Force += tempGravity * ParticleMass;
            }

            for (int i = 0; i < Neighbours.Count; i++)
            {
                FluidParticle a = Neighbours[i].A;
                FluidParticle b = Neighbours[i].B;

                if (a == b)
                    continue;

                Vector3 rv = a.Position - b.Position;
                float r = rv.Length();
                float r2 = r * r;

                // Force due to the fluid pressure gradient
                #region Pressure

                //Eq. 10 Müller03
                Vector3 pressureForce = -ParticleMass / b.Density * (a.Pressure + b.Pressure) / 2.0f * -SmoothKernel.SpikyGradient(rv); //La in ett minus här (-Smoothed...)

                #endregion

                // Force due to the viscosity of the fluid
                #region Viscosity

                Vector3 viscosityForce = Vector3.Zero;

                // Eq. 14 Müller03
                viscosityForce = Viscosity * ParticleMass * (b.Velocity - a.Velocity) / b.Density * SmoothKernel.ViscosityLaplacian(rv);

                #endregion

                // Müller03 model surface tension forces even thou it is not present in the Navier-Stokes equations (Eq. 7)
                #region Surface Tension

                Vector3 surfaceTensionForce = Vector3.Zero;

                // Müller03: "Evaluating n/|n| at locations where |n| is small causes numerical problems. We only evaluate the force if |n| exceeds a certain threshold
                float threshold = 0.05f;

                float colorFieldLaplacian = ParticleMass / a.Density * SmoothKernel.Poly6Laplacian(rv);
                Vector3 n = ParticleMass / a.Density * SmoothKernel.Poly6Gradient(rv); // <- The gradient field of the smoothed color field
                
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
            for (int i = 0; i < ActiveParticles; i++)
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

        public void Update(float elapsedTime, float t)
        {
            float timeStep = elapsedTime * t;

            StopWatch.Reset();
            StopWatch.Start();

            if (timeStep != 0)
            {
                // Clear particle properties
                for (int i = 0; i < ActiveParticles; i++)
                {
                    Particles[i].Force = Vector3.Zero;
                    //Particles[i].Density = 0;
                    Particles[i].Density = ParticleMass * this.Poly6Zero;
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

                for (int i = 0; i < ActiveParticles; i++)
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

        //private void TempFindNeighbors()
        //{
        //    Neighbours.Clear();
        //    for (int i = 0; i < ActiveParticles; i++)
        //    {
        //        for (int j = 0; j < Particles.Count; j++)
        //        {
        //            if (i == j)
        //                continue;

        //            float r = (Particles[i].Position - Particles[j].Position).Length();

        //            if (r < SmoothKernel.h)
        //            {
        //                Neighbours.Add(new NeighbourPair(Particles[i], Particles[j]));
        //            }
        //        }
        //    }
        //}

        public void HandleCollisions()
        {
            float bounce = 0.33f;
            Matrix invWorld = Matrix.Invert(Game1.glassBox.World);

            foreach (FluidParticle particle in Particles)
            {
                Vector3 particleTempPos = Vector3.Transform(particle.Position, invWorld); //Game1.glassBox.World);
                Vector3 particleTempVel = Vector3.Transform(particle.Velocity, invWorld); //Game1.glassBox.World);

                if (particleTempPos.Y < Bounds.Min.Y)
                {
                    particleTempVel.Y *= -1 * bounce;
                    particleTempPos.Y = Bounds.Min.Y;
                }

                if (particleTempPos.Y > Bounds.Max.Y)
                {
                    particleTempVel.Y *= -1 * bounce;
                    particleTempPos.Y = Bounds.Max.Y;
                }

                if (particleTempPos.X < Bounds.Min.X)
                {
                    particleTempVel.X *= -1 * bounce;
                    particleTempPos.X = Bounds.Min.X;
                }

                if (particleTempPos.X > Bounds.Max.X)
                {
                    particleTempVel.X *= -1 * bounce;
                    particleTempPos.X = Bounds.Max.X;
                }

                if (particleTempPos.Z > Bounds.Max.Z)
                {
                    particleTempVel.Z *= -1 * bounce;
                    particleTempPos.Z = Bounds.Max.Z;
                }

                if (particleTempPos.Z < Bounds.Min.Z)
                {
                    particleTempVel.Z *= -1 * bounce;
                    particleTempPos.Z = Bounds.Min.Z;
                }

                particle.Position = Vector3.Transform(particleTempPos, Game1.glassBox.World);
                particle.Velocity = Vector3.Transform(particleTempVel, Game1.glassBox.World);
            }
        }
    }
}