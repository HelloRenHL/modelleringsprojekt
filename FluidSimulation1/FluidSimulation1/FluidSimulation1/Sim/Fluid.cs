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

        AxisAlignedBoundingBox bounds;

        public Fluid(int maxParticles)
        {
            bounds = new AxisAlignedBoundingBox();
            //bounds.Set(-Vector3.One, Vector3.One);
            int size = 1;

            //bounds.Set(new Vector3(-1f, -1f, -1f) * size, new Vector3(1f, 1f, 1f) * size);
            
            bounds.Set(new Vector3(-1f, -0.5f, -0.2f) * size, new Vector3(1f, 0.5f, 0.2f) * size);
            FluidHash = new FluidHash(bounds, SmoothKernel.h);

            //bounds.Set(new Vector3(-1f, -0.5f, -0.2f) * size, new Vector3(1f, 0.5f, 0.2f) * size);
            //bounds.Max = Vector3.Transform(bounds.Max, Matrix.CreateRotationY(MathHelper.ToRadians(30)));
            //bounds.Min = Vector3.Transform(bounds.Min, Matrix.CreateRotationY(MathHelper.ToRadians(30)));

            MaxParticles = maxParticles;
            ActiveParticles = maxParticles;

            Poly6Zero = SmoothKernel.Poly6(0);

            InitializeParticles();
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
                particle.Position = Random(bounds.Min, bounds.Max);
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

                float density = ParticleMass * SmoothKernel.Poly6((b.Position - a.Position).Length());

                // Eq. 3 ifrån Müller03
                a.Density += density;
                b.Density += density;
            }
        }

        private void ComputePressure()
        {
            for (int i = 0; i < ActiveParticles; i++)
            {
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

                RotateParticles(Particles, 0.1f);
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

        private void HandleCollisions()
        {
            float bounce = 0.33f;

            for (int i = 0; i < ActiveParticles; i++)
            {


                if (Particles[i].Position.Y < bounds.Min.Y)
                {
                    Particles[i].Velocity.Y *= -1 * bounce;
                    Particles[i].Position.Y = bounds.Min.Y;
                }

                if (Particles[i].Position.Y > bounds.Max.Y)
                {
                    Particles[i].Velocity.Y *= -1 * bounce;
                    Particles[i].Position.Y = bounds.Max.Y;
                }

                if (Particles[i].Position.X < bounds.Min.X)
                {
                    Particles[i].Velocity.X *= -1 * bounce;
                    Particles[i].Position.X = bounds.Min.X;
                }

                if (Particles[i].Position.X > bounds.Max.X)
                {
                    Particles[i].Velocity.X *= -1 * bounce;
                    Particles[i].Position.X = bounds.Max.X;
                }

                if (Particles[i].Position.Z > bounds.Max.Z)
                {
                    Particles[i].Velocity.Z *= -1 * bounce;
                    Particles[i].Position.Z = bounds.Max.Z;
                }

                if (Particles[i].Position.Z < bounds.Min.Z)
                {
                    Particles[i].Velocity.Z *= -1 * bounce;
                    Particles[i].Position.Z = bounds.Min.Z;
                }
            }
        }
            public void RotateParticles(List<FluidParticle> particles,float amount)
        {
            float bounce = 0.33f;
            Matrix invWorld  = Matrix.Invert(Game1.glassBox.World);
            foreach(FluidParticle particle in particles){

                Vector3 particleTempPos = Vector3.Transform(particle.Position, invWorld); //Game1.glassBox.World);
                Vector3 particleTempVel = Vector3.Transform(particle.Velocity, invWorld); //Game1.glassBox.World);

                if (particleTempPos.Y < bounds.Min.Y)
                {
                    particleTempVel.Y *= -1 * bounce;
                    particleTempPos.Y = bounds.Min.Y;
                }

                if (particleTempPos.Y > bounds.Max.Y)
                {
                    particleTempVel.Y *= -1 * bounce;
                    particleTempPos.Y = bounds.Max.Y;
                }

                if (particleTempPos.X < bounds.Min.X)
                {
                    particleTempVel.X *= -1 * bounce;
                    particleTempPos.X = bounds.Min.X;
                }

                if (particleTempPos.X > bounds.Max.X)
                {
                    particleTempVel.X *= -1 * bounce;
                    particleTempPos.X = bounds.Max.X;
                }

                if (particleTempPos.Z > bounds.Max.Z)
                {
                    particleTempVel.Z *= -1 * bounce;
                    particleTempPos.Z = bounds.Max.Z;
                }

                if (particleTempPos.Z < bounds.Min.Z)
                {
                    particleTempVel.Z *= -1 * bounce;
                    particleTempPos.Z = bounds.Min.Z;
                }


                particle.Position = Vector3.Transform(particleTempPos, Game1.glassBox.World);
                particle.Velocity = Vector3.Transform(particleTempVel, Game1.glassBox.World);
            }

        }
        }

        
        
    }

