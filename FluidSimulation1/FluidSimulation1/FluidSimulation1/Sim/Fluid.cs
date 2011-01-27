using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FluidSimulation1
{
    public class Fluid
    {
        public FluidParticle[] FluidParticles;
        public static Random Random = new Random();
        public int MaxParticles;
        public int ActiveParticles;
        public float Viscosity = 10f;
        public float SurfaceTension = 12.5f;

        public Fluid(int maxParticles)
        {
            MaxParticles = maxParticles;

            FluidParticles = new FluidParticle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
            {
                FluidParticles[i] = new FluidParticle();
            }
        }

        public float Wpoly6(Vector3 rv)
        {
            return 4.075296E+07f * (float)Math.Pow(0.0225f - rv.LengthSquared(), 3);
        }

        //procedure compute_all_forces() { [...] }

        //procedure find_neighbors_of_fluid_particles()
        //{
        //    clear(grids);
        //    for each fluid particle i
        //    {
        //        //compute voxel index corresponding to particle i
        //        g = compute_index(i);

        //        for each particle j in neighborhood of voxel[g]
        //        {
        //            dist = compute_distance(i, j);
        //            if(dist < h)
        //            {
        //                // store neighbor particle and its distance
        //                add(neighbor_list[i], j, dist)
        //            }
        //        }

        //        //allocate particle to corresponding voxel
        //        add(voxels[g], i);
        //    }
        //}

        //procedure update_positons_and_velocity(int i)
        //{
        //    //the simulation stores position, velocity,
        //    //velocity half stepped,
        //    //and acceleration for each fluid particle
        //    for each fluid particle i
        //    {
        //        //compute v(t + 1/2*dt)
        //        vel_half_next = vel_half[i] + t*acc[i];

        //        //compute r(t + dt)
        //        pos[i] = pos[i] + t*v_half_next;

        //        //compute v(t)
        //        vel[i] = 0.5*(vel_half_next + vel_half[i]);
        //        vel_half[i] = vel_half_next;
        //    }
        //}


        #region temp
        //public void Simulate(float timeStep)
        //{
        //    if (timeStep != 0f)
        //    {

        //        for (int i = 0; i < this.m_numActiveParticles; i++)
        //        {
        //            this.m_particles[i].velocity += (Vector3)(this.m_particles[i].force * (timeStep * this.m_particles[i].densityReciprocal));
        //            this.m_particles[i].position += (Vector3)(this.m_particles[i].velocity * timeStep);
        //            this.m_particles[i].life += timeStep;
        //        }


        //        int count = this.m_obstacles.Count;
        //        for (int j = 0; j < count; j++)
        //        {
        //            this.m_obstacles[j].HandleCollisions(ref this.m_particles);
        //        }


        //        for (int k = 0; k < this.m_numActiveParticles; k++)
        //        {
        //            this.m_particles[k].force = (Vector3)(this.m_gravityDirection * this.m_gravityForce);
        //            this.m_particles[k].density = this.m_particles[k].mass * this.Wpoly6Zero;
        //        }



        //        this.FindNeighbors();
        //        for (int m = 0; m < this.m_neighborList.Count; m++)
        //        {
        //            FluidParticle a = this.m_neighborList.Data[m].a;
        //            FluidParticle b = this.m_neighborList.Data[m].b;
        //            Vector3 rv = a.position - b.position;

        //            float num6 = this.Wpoly6(rv);

        //            a.density += b.mass * num6;
        //            b.density += a.mass * num6;
        //        }


        //        //if (this.m_isColorMixing)
        //        //{
        //        //    for (int num7 = 0; num7 < this.m_neighborList.Count; num7++)
        //        //    {
        //        //        FluidParticle particle3 = this.m_neighborList.Data[num7].a;
        //        //        FluidParticle particle4 = this.m_neighborList.Data[num7].b;
        //        //        float num8 = this.Wpoly6(particle3.position - particle4.position) / 2000f;
        //        //        Vector4 vector2 = (Vector4)((particle3.color + particle4.color) * 0.5f);
        //        //        particle3.color = Vector4.Lerp(particle3.color, vector2, num8);
        //        //        particle4.color = Vector4.Lerp(particle4.color, vector2, num8);
        //        //    }
        //        //}


        //        for (int n = 0; n < this.m_numActiveParticles; n++)
        //        {
        //            this.m_particles[n].densityReciprocal = 1f / this.m_particles[n].density;
        //            this.m_particles[n].surfaceNormal = this.m_particles[n].mass * this.m_particles[n].densityReciprocal;
        //            this.m_particles[n].pressure = 0.2f * (this.m_particles[n].density - 1000f);
        //        }


        //        float viscosity = this.m_viscosity;
        //        float surfaceTension = this.m_surfaceTension;
        //        for (int num12 = 0; num12 < this.m_neighborList.Count; num12++)
        //        {
        //            FluidParticle particle5 = this.m_neighborList.Data[num12].a;
        //            FluidParticle particle6 = this.m_neighborList.Data[num12].b;

        //            if (particle5.position != particle6.position)
        //            {
        //                Vector3 vector5;
        //                Vector3 vector6;
        //                float num13;
        //                Vector3 vector3 = new Vector3(0f);
        //                Vector3 vector4 = particle5.position - particle6.position;
        //                this.GetGradientNormalAndLaplacian(ref vector4, out vector5, out vector6, out num13);
        //                float num14 = (particle5.pressure + particle6.pressure) * 0.5f;


        //                vector3 -= (Vector3)(vector5 * num14);
        //                Vector3 vector7 = particle6.velocity - particle5.velocity;
        //                vector3 += (Vector3)(vector7 * (num13 * viscosity));
        //                vector3 += (Vector3)(vector6 * (num13 * surfaceTension));

        //                particle5.force += (Vector3)(vector3 * particle6.surfaceNormal);
        //                particle6.force -= (Vector3)(vector3 * particle5.surfaceNormal);
        //            }
        //        }
        //    }
        //}
        #endregion

        public List<Particle> FindNeighbours()
        {
            return new List<Particle>();
        }


        /// <summary>
        /// Saxat från GPG 6
        /// </summary>
        /// <param name="Neighbours"></param>
        private void ComputeDensity(List<Particle> Neighbours)
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                for (int j = 0; j < Neighbours.Count; j++)
                {
                    if (j == i)
                        continue;

                    float distance = Vector3.Distance(Neighbours[i].Position, Neighbours[j].Position);

                    float density = 0;

                    Neighbours[i].Density += density;
                    Neighbours[j].Density += density;
                }
            }
        }


        public void Update(float elapsedTime)
        {
            
            //Find neighbours of particles
            List<Particle> Neighbours = FindNeighbours();


            //        compute_particle_densities
            ComputeDensity(Neighbours);


            //including rigid/fluid interactions
            //ComputePressures(Neighbours);


            //ComputeAllForces(Neighbours);

            //        compute_all_forces //including static collisions and gravity

            //        //update position and velocity of each fluid particle according to Leap-Frog scheme
            //        for each fluid particle i
            //        {
            //            update_position_and_velocity(i)
            //        }

            //        for each rigid body r
            //        {
            //            //perform initial rigid body particle update, and then compute updated position and orientation of the whole rigid body.
            //            compute_rigid_body_motion(r)
            //            //now update the individual rigid particle positions to enforce rigidity.
            //            for each rigid particle i in r
            //            {
            //                //update position and velocity of each rigid particle
            //                // by Equation 2.6.20 to enforce rigidity // Eq: rj(t + dt) = R(t + dt)(rj(t)-rg(t)) + rg(t + dt) // s.199
            //                update_rigid_positions_and_velocity(i)
            //            }

            //        }
           
        }


    }
}
