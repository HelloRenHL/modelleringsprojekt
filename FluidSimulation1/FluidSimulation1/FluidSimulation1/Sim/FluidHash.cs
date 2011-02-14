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

        AxisAlignedBoundingBox AxisAlignedBoundingBox;

        private float h;
        private float h2;

        int cellSizeX;
        int cellSizeY;
        int cellSizeZ;

        public FluidHash(AxisAlignedBoundingBox axisAlignedBoundingBox, float h)
        {
            this.AxisAlignedBoundingBox = axisAlignedBoundingBox;

            this.h = h;
            this.h2 = h * h;

            this.cellSizeX = (int)Math.Ceiling((double)((this.AxisAlignedBoundingBox.Max.X - this.AxisAlignedBoundingBox.Min.X) / this.h));
            this.cellSizeY = (int)Math.Ceiling((double)((this.AxisAlignedBoundingBox.Max.Y - this.AxisAlignedBoundingBox.Min.Y) / this.h));
            this.cellSizeZ = (int)Math.Ceiling((double)((this.AxisAlignedBoundingBox.Max.Z - this.AxisAlignedBoundingBox.Min.Z) / this.h));


            // Create the list that holds each cells particles
            this.numberOfEntries = this.cellSizeX * this.cellSizeY * this.cellSizeZ;
            this.Entry = new List<FluidParticle>[this.numberOfEntries];
            for (int i = 0; i < this.numberOfEntries; i++)
            {
                this.Entry[i] = new List<FluidParticle>(64);
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
            x1 = Math.Abs(x1) % this.cellSizeX;
            y1 = Math.Abs(y1) % this.cellSizeY;
            z1 = Math.Abs(z1) % this.cellSizeZ;

            int index = (((z1 * this.cellSizeY) + y1) * this.cellSizeX) + x1;

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
            x = Math.Abs(x) % this.cellSizeX;
            y = Math.Abs(y) % this.cellSizeY;
            z = Math.Abs(z) % this.cellSizeZ;

            int index = (((z * this.cellSizeY) + y) * this.cellSizeX) + x;

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
            for (int i = 0; i < this.cellSizeZ; i++)
            {
                for (int j = 0; j < this.cellSizeY; j++)
                {
                    for (int k = 0; k < this.cellSizeX; k++)
                    {
                        this.GatherCellNeighbors(k, j, i, neighbors);
                    }
                }
            }
        }

        public void AddParticle(FluidParticle p)
        {
            //Debug.Write(p.Position.ToString());

            int x = (int)((p.Position.X - this.AxisAlignedBoundingBox.Min.X) / this.h);
            int y = (int)((p.Position.Y - this.AxisAlignedBoundingBox.Min.Y) / this.h);
            int z = (int)((p.Position.Z - this.AxisAlignedBoundingBox.Min.Z) / this.h);

            x = Math.Abs(x) % this.cellSizeX;
            y = Math.Abs(y) % this.cellSizeY;
            z = Math.Abs(z) % this.cellSizeZ;

            int index = (((z * this.cellSizeY) + y) * this.cellSizeX) + x;
            this.Entry[index].Add(p);
        }
    }
}
