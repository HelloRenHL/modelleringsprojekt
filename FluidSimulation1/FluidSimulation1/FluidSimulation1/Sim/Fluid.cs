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
        public int ActiveParticles;
        public float Viscosity = 10f;
        public float SurfaceTension = 12.5f;

        public FluidHash FluidHash;

        private int activeParticles;

        List<NeighbourPair> Neighbours = new List<NeighbourPair>();

        public List<FluidParticle> Particles = new List<FluidParticle>();
        float Poly6Zero;

        public Vector3 Gravity = Vector3.Down * 196.2f;

        float h;
        float h2;
        float h6;
        float h9;

        AxisAlignedBoundingBox bounds;

        public Fluid(int maxParticles)
        {
            h = 0.15f; //0.15f
            h2 = h * h;
            h6 = (float)Math.Pow(h, 6);
            h9 = (float)Math.Pow(h, 9);

            bounds = new AxisAlignedBoundingBox();
            bounds.Set(-Vector3.One, Vector3.One);

            FluidHash = new FluidHash(bounds, h);

            MaxParticles = maxParticles;
            activeParticles = maxParticles;

            Poly6Zero = SmoothKernel.Poly6(Vector3.Zero, h9, h2);

            for (int i = 0; i < maxParticles; i++)
            {
                Particles.Add(new FluidParticle());
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

        private void ComputeDensity()
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                FluidParticle a = Neighbours[i].A;
                FluidParticle b = Neighbours[i].B;

                float density = a.Mass * SmoothKernel.Poly6(a.Position - b.Position, h9, h2);

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
                Particles[i].Force += this.Gravity * Particles[i].Mass; // / Particles[i].Density
            }

            for (int i = 0; i < Neighbours.Count; i++)
            {
                FluidParticle a = Neighbours[i].A;
                FluidParticle b = Neighbours[i].B;

                Vector3 rv = a.Position - b.Position;
                float r = rv.Length();

                if (r < 0.1f)
                    continue;

                float r2 = r * r;

                // Force due to the fluid pressure gradient
                #region Pressure
                //Eq. 10 Müller03
                Vector3 pressureForce = -b.Mass * (a.Pressure + b.Pressure) / (2 * b.Density) * SmoothKernel.SpikyGradient(rv, r, r2, h, h2, h6);
                #endregion

                // Force due to the viscosity of the fluid
                #region Viscosity

                Vector3 viscosityForce = Vector3.Zero;

                // Eq. 14 Müller03
                viscosityForce = Viscosity * b.Mass * (b.Velocity - a.Velocity) / b.Density * SmoothKernel.ViscosityLaplacian(r, h, h6); // <- Får allt att hacka

                #endregion

                // Müller03 model surface tension forces even thou it is not present in the Navier-Stokes equations (Eq. 7)
                #region Surface Tension

                // Müller03: "Evaluating n/|n| at locations where |n| is small causes numerical problems. We only evaluate the force if |n| exceeds a certain threshold
                Vector3 surfaceTensionForce = Vector3.Zero;

                float threshold = 0.05f;

                float colorFieldLaplacian = a.Mass / a.Density * SmoothKernel.Poly6Laplacian(r2, h2, h9);
                Vector3 n = a.Mass / a.Density * SmoothKernel.Poly6Gradient(rv, r2, h2, h9); // <- The gradient field of the smoothed color field
                
                if (n.Length() > threshold)
                {
                    surfaceTensionForce = -SurfaceTension * colorFieldLaplacian * n / n.Length();
                }

                #endregion

                a.Force += pressureForce + viscosityForce + surfaceTensionForce;
                b.Force -= pressureForce + viscosityForce + surfaceTensionForce;
            }
        }

        // Funkade skitdåligt?
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

        /*public void test()
        {
            if (timeStep != 0f)
            {
                for (int i = 0; i < this.m_numActiveParticles; i++)
                {
                    FluidParticle particle1 = this.m_particles[i];
                    particle1.velocity += (Vector3)(this.m_particles[i].force * (timeStep * this.m_particles[i].densityReciprocal));
                    FluidParticle particle7 = this.m_particles[i];
                    particle7.position += (Vector3)(this.m_particles[i].velocity * timeStep);
                    FluidParticle particle8 = this.m_particles[i];
                    particle8.life += timeStep;
                }
                int count = this.m_obstacles.Count;
                for (int j = 0; j < count; j++)
                {
                    this.m_obstacles[j].HandleCollisions(ref this.m_particles);
                }
                for (int k = 0; k < this.m_numActiveParticles; k++)
                {
                    this.m_particles[k].force = (Vector3)(this.m_gravityDirection * this.m_gravityForce);
                    this.m_particles[k].density = this.m_particles[k].mass * this.Wpoly6Zero;
                }
                this.FindNeighbors();
                for (int m = 0; m < this.m_neighborList.Count; m++)
                {
                    FluidParticle a = this.m_neighborList.Data[m].a;
                    FluidParticle b = this.m_neighborList.Data[m].b;
                    Vector3 rv = a.position - b.position;
                    float num6 = this.Wpoly6(rv);
                    a.density += b.mass * num6;
                    b.density += a.mass * num6;
                }
                if (this.m_isColorMixing)
                {
                    for (int num7 = 0; num7 < this.m_neighborList.Count; num7++)
                    {
                        FluidParticle particle3 = this.m_neighborList.Data[num7].a;
                        FluidParticle particle4 = this.m_neighborList.Data[num7].b;
                        float num8 = this.Wpoly6(particle3.position - particle4.position) / 2000f;
                        Vector4 vector2 = (Vector4)((particle3.color + particle4.color) * 0.5f);
                        particle3.color = Vector4.Lerp(particle3.color, vector2, num8);
                        particle4.color = Vector4.Lerp(particle4.color, vector2, num8);
                    }
                }
                for (int n = 0; n < this.m_numActiveParticles; n++)
                {
                    this.m_particles[n].densityReciprocal = 1f / this.m_particles[n].density;
                    this.m_particles[n].surfaceNormal = this.m_particles[n].mass * this.m_particles[n].densityReciprocal;
                    this.m_particles[n].pressure = 0.2f * (this.m_particles[n].density - 1000f);
                }
                float viscosity = this.m_viscosity;
                float surfaceTension = this.m_surfaceTension;
                for (int num12 = 0; num12 < this.m_neighborList.Count; num12++)
                {
                    FluidParticle particle5 = this.m_neighborList.Data[num12].a;
                    FluidParticle particle6 = this.m_neighborList.Data[num12].b;
                    if (particle5.position != particle6.position)
                    {
                        Vector3 vector5;
                        Vector3 vector6;
                        float num13;
                        Vector3 vector3 = new Vector3(0f);
                        Vector3 vector4 = particle5.position - particle6.position;
                        this.GetGradientNormalAndLaplacian(ref vector4, out vector5, out vector6, out num13);
                        float num14 = (particle5.pressure + particle6.pressure) * 0.5f;
                        vector3 -= (Vector3)(vector5 * num14);
                        Vector3 vector7 = particle6.velocity - particle5.velocity;
                        vector3 += (Vector3)(vector7 * (num13 * viscosity));
                        vector3 += (Vector3)(vector6 * (num13 * surfaceTension));
                        particle5.force += (Vector3)(vector3 * particle6.surfaceNormal);
                        particle6.force -= (Vector3)(vector3 * particle5.surfaceNormal);
                    }
                }
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
                    Particles[i].Density = Particles[i].Mass * this.Poly6Zero;
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
                //UpdateParticles(timeStep);

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
