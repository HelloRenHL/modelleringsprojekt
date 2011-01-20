using System;
using Microsoft.Xna.Framework;

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

        //public void compute_particle_densities()
        //{
        //    for each particle i
        //    {
        //        for each particle j, dist in neighbor_list[i]
        //        {
        //            d = compute_density(dist, pos[i], pos[j])

        //            // add contribution to density of one particle
        //            // and another because of symmetry of contribution
        //            density[i] += d;
        //            density[j] += d;
        //        }
        //    }
        //}

        public void Update(float elapsedTime)
        {
            //while simulating
            //{
            //    find_neighbours_of_fluid_particles

            //        compute_particle_densities
            //        compute_pressures //including rigid/fluid interactions

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
            //}
        }
    }
}
