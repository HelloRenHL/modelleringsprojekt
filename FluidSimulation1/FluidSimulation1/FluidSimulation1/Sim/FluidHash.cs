using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace FluidSimulation1
{
    public class FluidHash
    {
        public List<FluidParticle>[] Entry;
        private int numberOfEntries;

        private float h;
        private float h2;

        int width;
        int height;
        int depth;

        Fluid fluid;

        public FluidHash(Fluid fluid)
        {
            this.fluid = fluid;

            this.h = SmoothKernel.h;
            this.h2 = h * h;

            // Calculate number of cells required
            this.width = (int)Math.Ceiling((double)((fluid.Container.Bounds.Max.X - fluid.Container.Bounds.Min.X) / this.h)); // Ceiling avrundar alltid uppåt
            this.height = (int)Math.Ceiling((double)((fluid.Container.Bounds.Max.Y - fluid.Container.Bounds.Min.Y) / this.h)); // 4.001 blir tex 5
            this.depth = (int)Math.Ceiling((double)((fluid.Container.Bounds.Max.Z - fluid.Container.Bounds.Min.Z) / this.h));

            // Create a list that holds lists of neighbours
            this.numberOfEntries = this.width * this.height * this.depth;
            this.Entry = new List<FluidParticle>[this.numberOfEntries];
            for (int i = 0; i < this.numberOfEntries; i++)
            {
                this.Entry[i] = new List<FluidParticle>(64); // Max number of neighbours
            }
        }

        public void Clear()
        {
            for (int i = 0; i < this.numberOfEntries; i++)
            {
                this.Entry[i].Clear();
            }
        }

        private void GatherCellNeighbors2(FluidParticle part, int x1, int y1, int z1, List<NeighbourPair> neighbors)
        {
            x1 = Math.Abs(x1) % this.width;
            y1 = Math.Abs(y1) % this.height;
            z1 = Math.Abs(z1) % this.depth;

            int index = (((z1 * this.height) + y1) * this.width) + x1;

            foreach (FluidParticle particle in this.Entry[index])
            {
                Vector3 rv = part.Position - particle.Position;

                if (rv.LengthSquared() < this.h2)
                {
                    neighbors.Add(new NeighbourPair(part, particle));
                }
            }
        }


        private void SelfGatherCellNeighbors(FluidParticle part, int index, int index0, List<NeighbourPair> neighbors)
        {
            for (int i = index + 1; i < this.Entry[index0].Count; i++)
            {
                FluidParticle b = this.Entry[index0][i];
                Vector3 rv = part.Position - b.Position;

                if (rv.LengthSquared() < this.h2)
                {
                    neighbors.Add(new NeighbourPair(part, b));
                }
            }
        }

        private void GatherCellNeighbors(int x, int y, int z, List<NeighbourPair> neighbors)
        {
            // Get the cell index
            x = Math.Abs(x) % this.width;
            y = Math.Abs(y) % this.height;
            z = Math.Abs(z) % this.depth;

            int index = (((z * this.height) + y) * this.width) + x;

            int counter = 0;

            foreach (FluidParticle particle in this.Entry[index])
            {
                this.SelfGatherCellNeighbors(particle, counter, index, neighbors);

                this.GatherCellNeighbors2(particle, x + 1, y, z, neighbors);
                this.GatherCellNeighbors2(particle, x, y + 1, z, neighbors);
                this.GatherCellNeighbors2(particle, x + 1, y + 1, z, neighbors);
                this.GatherCellNeighbors2(particle, x, y, z + 1, neighbors);
                this.GatherCellNeighbors2(particle, x + 1, y, z + 1, neighbors);
                this.GatherCellNeighbors2(particle, x, y + 1, z + 1, neighbors);
                this.GatherCellNeighbors2(particle, x + 1, y + 1, z + 1, neighbors);

                counter++;
            }
        }

        public void GatherNeighbors(List<NeighbourPair> neighbors)
        {
            // Loop through each cell
            for (int i = 0; i < this.depth; i++)
            {
                for (int j = 0; j < this.height; j++)
                {
                    for (int k = 0; k < this.width; k++)
                    {
                        this.GatherCellNeighbors(k, j, i, neighbors);
                    }
                }
            }
        }

        public void AddParticle(FluidParticle p)
        {
            // Calculate which cell this particle goes in
            int x = (int)((p.Position.X - fluid.Container.Bounds.Max.X) / this.h);
            int y = (int)((p.Position.Y - fluid.Container.Bounds.Max.Y) / this.h);
            int z = (int)((p.Position.Z - fluid.Container.Bounds.Max.Z) / this.h);

            x = Math.Abs(x) % this.width;
            y = Math.Abs(y) % this.height;
            z = Math.Abs(z) % this.depth;

            int index = this.width * this.height * z + this.width * y + x;

            this.Entry[index].Add(p);
        }
    }
}
