using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidSimulation1.Sim;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class MarchingCubesField
    {
        private float bx;
        private float by;
        private float bz;

        private float dx;
        private float dy;
        private float dz;

        private float[] divLut;

        private float[] fieldSmooth;
        public float[] isInside;

        public int NumTriangles = 0;
        public int NumVertexes = 0;

        public MarchingCubesTriangle[] Triangles;
        public MarchingCubesVertex[] Vertexes;

        private int MAX_CUBE_TRIANGLES = 5;
        private bool SMOOTH = true;

        private int X;
        private int Y;
        private int Z;

        private MarchingCubesVertex[] xv;
        private MarchingCubesVertex[] yv;
        private MarchingCubesVertex[] zv;

        public MarchingCubesField(int X_, int Y_, int Z_, float bx_, float by_, float bz_, float dx_, float dy_, float dz_)
        {
            this.X = X_;
            this.Y = Y_;
            this.Z = Z_;

            this.isInside = new float[this.X * this.Y * this.Z];
            this.fieldSmooth = new float[this.X * this.Y * this.Z];

            this.xv = new MarchingCubesVertex[(this.X - 1) * this.Y * this.Z];
            this.yv = new MarchingCubesVertex[this.X * (this.Y - 1) * this.Z];
            this.zv = new MarchingCubesVertex[this.X * this.Y * (this.Z - 1)];

            for (int i = 0; i < this.xv.Length; i++)
            {
                this.xv[i] = new MarchingCubesVertex();
            }

            for (int j = 0; j < this.yv.Length; j++)
            {
                this.yv[j] = new MarchingCubesVertex();
            }

            for (int k = 0; k < this.zv.Length; k++)
            {
                this.zv[k] = new MarchingCubesVertex();
            }

            this.bx = bx_;
            this.by = by_;
            this.bz = bz_;
            this.dx = dx_;
            this.dy = dy_;
            this.dz = dz_;

            this.Triangles = new MarchingCubesTriangle[this.MAX_CUBE_TRIANGLES * (this.X - 1) * (this.Y - 1) * (this.Z - 1)];
            for (int i = 0; i < this.Triangles.Length; i++)
            {
                this.Triangles[i] = new MarchingCubesTriangle();
            }

            this.Vertexes = new MarchingCubesVertex[(this.X - 1) * this.Y * this.Z + this.X * (this.Y - 1) * this.Z + this.X * this.Y * (this.Z - 1)];
            for (int n = 0; n < this.Vertexes.Length; n++)
            {
                this.Vertexes[n] = new MarchingCubesVertex();
            }

            this.divLut = new float[20];
            this.divLut[0] = 0;
            for (int i = 1; i < 20; i++)
            {
                this.divLut[i] = 1f / (i * 3f);
            }
        }

        private Vector3 Bisect(float left, float right, Vector3 pleft, Vector3 pright)
        {
            float num = left / (left - right);
            return pleft + (pright - pleft) * num;
        }

        public void CreateColorField(Fluid fluid)
        {
            for (int n = 0; n < X * Y * Z; n++)
            {
                isInside[n] = FluidRendererMarchingCubes.MinimumParticleSize;
            }

            for (int n = 0; n < fluid.ActiveParticles; n++)
            {
                FluidParticle particle = fluid.Particles[n];

                Vector3 position = particle.Position;

                float h = SmoothKernel.h;

                int num5 = (int)((position.X - h - bx) / dx);
                int num6 = (int)((position.Y - h - by) / dy);
                int num7 = (int)((position.Z - h - bz) / dz);
                int num8 = (int)((position.X + h - bx) / dx);
                int num9 = (int)((position.Y + h - by) / dy);
                int num10 = (int)((position.Z + h - bz) / dz);

                if (num5 < 0)
                {
                    num5 = 0;
                }
                else if (num5 >= X)
                {
                    num5 = X - 1;
                }

                if (num6 < 0)
                {
                    num6 = 0;
                }
                else if (num6 >= Y)
                {
                    num6 = Y - 1;
                }

                if (num7 < 0)
                {
                    num7 = 0;
                }
                else if (num7 >= Z)
                {
                    num7 = Z - 1;
                }

                if (num8 < 0)
                {
                    num8 = 0;
                }
                else if (num8 >= X)
                {
                    num8 = X - 1;
                }

                if (num9 < 0)
                {
                    num9 = 0;
                }
                else if (num9 >= Y)
                {
                    num9 = Y - 1;
                }

                if (num10 < 0)
                {
                    num10 = 0;
                }
                else if (num10 >= Z)
                {
                    num10 = Z - 1;
                }

                for (int i = num7; i <= num10; i++)
                {
                    for (int j = num6; j <= num9; j++)
                    {
                        for (int k = num5; k <= num8; k++)
                        {
                            Vector3 p = this.GCSToWCS(k, j, i);

                            if (fluid.Container.Bounds.Contains(p) == ContainmentType.Contains)
                            {
                                Vector3 rv = position - p;
                                if (rv.LengthSquared() < 0.0225f)
                                {
                                    isInside[(i * Y + j) * X + k] -= SmoothKernel.Poly6(rv) * particle.SurfaceNormal;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ComputeGrids(Fluid fluid)
        {
            CreateColorField(fluid);
   
            this.SmoothColorField();
   
            this.NumVertexes = 0;
            for (int i = 0; i < this.Z; i++)
            {
                for(int j = 0; j < this.Y; j++)
                {
                    for(int k = 0; k < this.X - 1; k++)
                    {
                        int index = (i * this.Y + j) * this.X + k;
                        int num6 = (i * this.Y + j) * this.X + k + 1;
                        int num7 = (i * this.Y + j) * (this.X - 1) + k;

                        if (this.isInside[index] * this.isInside[num6] < 0f)
                        {
                            this.xv[num7].Position = this.Bisect(this.isInside[index], this.isInside[num6], this.GCSToWCS(k, j, i), this.GCSToWCS(k + 1, j, i));
                            this.xv[num7].Normal = Vector3.Zero;
                            this.Vertexes[this.NumVertexes] = this.xv[num7];
                            this.NumVertexes++;
                        }
                    }
                }
            }

            for (int i = 0; i < this.Z; i++)
            {
                for (int j = 0; j < this.Y - 1; j++)
                {
                    for (int k = 0; k < this.X; k++)
                    {
                        int num8 = (((i * this.Y) + j) * this.X) + k;
                        int num9 = (((i * this.Y) + (j + 1)) * this.X) + k;
                        int num10 = (((i * (this.Y - 1)) + j) * this.X) + k;

                        if ((this.isInside[num8] * this.isInside[num9]) < 0f)
                        {
                            this.yv[num10].Position = this.Bisect(this.isInside[num8], this.isInside[num9], this.GCSToWCS(k, j, i), this.GCSToWCS(k, j + 1, i));
                            this.yv[num10].Normal = Vector3.Zero;
                            this.Vertexes[this.NumVertexes] = this.yv[num10];
                            this.NumVertexes++;
                        }
                    }
                }
            }

            for (int i = 0; i < this.Z - 1; i++)
            {
                for (int j = 0; j < this.Y; j++)
                {
                    for (int k = 0; k < this.X; k++)
                    {
                        int num11 = (((i * this.Y) + j) * this.X) + k;
                        int num12 = ((((i + 1) * this.Y) + j) * this.X) + k;
                        int num13 = (((i * this.Y) + j) * this.X) + k;

                        if ((this.isInside[num11] * this.isInside[num12]) < 0f)
                        {
                            this.zv[num13].Position = this.Bisect(this.isInside[num11], this.isInside[num12], this.GCSToWCS(k, j, i), this.GCSToWCS(k, j, i + 1));
                            this.zv[num13].Normal = Vector3.Zero;
                            this.Vertexes[this.NumVertexes] = this.zv[num13];
                            this.NumVertexes++;
                        }
                    }
                }
            }
        }

        public Vector3 GCSToWCS(float x, float y, float z)
        {
            return new Vector3((x + 0.5f) * this.dx + this.bx,
                (y + 0.5f) * this.dy + this.by,
                (z + 0.5f) * this.dz + this.bz);
        }

        /// <summary>
        /// Clamp a value between 0 and max
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int Clamp(int value, int max)
        {
            return (int)MathHelper.Clamp(value, 0, max);
        }

        private void SmoothColorField()
        {
            for (int i = 0; i < this.X * this.Y * this.Z; i++)
            {
                fieldSmooth[i] = isInside[i];
            }

            //for (int k = 0; k < this.Z - 1; k++)
            //{
            //    for (int j = 0; j < this.Y; j++)
            //    {
            //        for (int i = 0; i < this.X; i++)
            //        {
            //            int a = (k * this.Y + j) * this.X + this.Clamp(i + 1, this.X);
            //            int b = (k * this.Y) + j * this.X + this.Clamp(i - 1, this.X);
            //            int c = (k * this.Y + this.Clamp(j + 1, this.Y)) * this.X + i;
            //            int d = (k * this.Y + this.Clamp(j - 1, this.Y)) * this.X + i;
            //            int e = (this.Clamp(k + 1, this.Z) * this.Y + j) * this.X + i;
            //            int f = (this.Clamp(k - 1, this.Z) * this.Y + j) * this.X + i;

            //            int temp = (k * this.Y + j) * this.X + i;
            //            this.isInside[temp] = (fieldSmooth[a] + fieldSmooth[b] + fieldSmooth[c] + fieldSmooth[d] + fieldSmooth[e] + fieldSmooth[f]) / 6f;
            //        }
            //    }
            //}
        }


        private void PolygonizeCube(int i, int j, int k, MarchingCubesVertex[] vertlist)
        {
            int index = 0;
            if (this.isInside[(k * this.Y + j) * this.X + i] < 0f)
            {
                index |= 1;
            }
            if (this.isInside[(((k * this.Y) + j) * this.X) + (i + 1)] < 0f)
            {
                index |= 2;
            }
            if (this.isInside[((((k + 1) * this.Y) + j) * this.X) + (i + 1)] < 0f)
            {
                index |= 4;
            }
            if (this.isInside[((((k + 1) * this.Y) + j) * this.X) + i] < 0f)
            {
                index |= 8;
            }
            if (this.isInside[(((k * this.Y) + (j + 1)) * this.X) + i] < 0f)
            {
                index |= 0x10;
            }
            if (this.isInside[(((k * this.Y) + (j + 1)) * this.X) + (i + 1)] < 0f)
            {
                index |= 0x20;
            }
            if (this.isInside[((((k + 1) * this.Y) + (j + 1)) * this.X) + (i + 1)] < 0f)
            {
                index |= 0x40;
            }
            if (this.isInside[((((k + 1) * this.Y) + (j + 1)) * this.X) + i] < 0f)
            {
                index |= 0x80;
            }
            if (Data.edgeTable[index] != 0)
            {
                if ((Data.edgeTable[index] & 1) != 0)
                {
                    vertlist[0] = this.xv[(((k * this.Y) + j) * (this.X - 1)) + i];
                }
                if ((Data.edgeTable[index] & 2) != 0)
                {
                    vertlist[1] = this.zv[(((k * this.Y) + j) * this.X) + (i + 1)];
                }
                if ((Data.edgeTable[index] & 4) != 0)
                {
                    vertlist[2] = this.xv[((((k + 1) * this.Y) + j) * (this.X - 1)) + i];
                }
                if ((Data.edgeTable[index] & 8) != 0)
                {
                    vertlist[3] = this.zv[(((k * this.Y) + j) * this.X) + i];
                }
                if ((Data.edgeTable[index] & 0x10) != 0)
                {
                    vertlist[4] = this.xv[(((k * this.Y) + (j + 1)) * (this.X - 1)) + i];
                }
                if ((Data.edgeTable[index] & 0x20) != 0)
                {
                    vertlist[5] = this.zv[(((k * this.Y) + (j + 1)) * this.X) + (i + 1)];
                }
                if ((Data.edgeTable[index] & 0x40) != 0)
                {
                    vertlist[6] = this.xv[((((k + 1) * this.Y) + (j + 1)) * (this.X - 1)) + i];
                }
                if ((Data.edgeTable[index] & 0x80) != 0)
                {
                    vertlist[7] = this.zv[(((k * this.Y) + (j + 1)) * this.X) + i];
                }
                if ((Data.edgeTable[index] & 0x100) != 0)
                {
                    vertlist[8] = this.yv[(((k * (this.Y - 1)) + j) * this.X) + i];
                }
                if ((Data.edgeTable[index] & 0x200) != 0)
                {
                    vertlist[9] = this.yv[(((k * (this.Y - 1)) + j) * this.X) + (i + 1)];
                }
                if ((Data.edgeTable[index] & 0x400) != 0)
                {
                    vertlist[10] = this.yv[((((k + 1) * (this.Y - 1)) + j) * this.X) + (i + 1)];
                }
                if ((Data.edgeTable[index] & 0x800) != 0)
                {
                    vertlist[11] = this.yv[((((k + 1) * (this.Y - 1)) + j) * this.X) + i];
                }
                for (int m = 0; Data.triTable[index, m] != -1; m += 3)
                {
                    this.Triangles[this.NumTriangles].v0 = vertlist[Data.triTable[index, m]];
                    this.Triangles[this.NumTriangles].v1 = vertlist[Data.triTable[index, m + 1]];
                    this.Triangles[this.NumTriangles].v2 = vertlist[Data.triTable[index, m + 2]];
                    this.NumTriangles++;
                }
            }
        }

        private void PolygonizeCubes()
        {
            this.NumTriangles = 0;
            MarchingCubesVertex[] vertlist = new MarchingCubesVertex[12];

            for (int i = 0; i < this.Z - 1; i++)
            {
                for (int j = 0; j < this.Y - 1; j++)
                {
                    for (int k = 0; k < this.X - 1; k++)
                    {
                        this.PolygonizeCube(k, j, i, vertlist);
                    }
                }
            }
        }


        private void SmoothMesh()
        {
            for (int i = 0; i < this.NumVertexes; i++)
            {
                this.Vertexes[i].Temp = Vector3.Zero;
                this.Vertexes[i].NumTrisConnected = 0;
            }

            for (int i = 0; i < this.NumTriangles; i++)
            {
                Vector3 vector = (this.Triangles[i].v0.Position + this.Triangles[i].v1.Position) + this.Triangles[i].v2.Position;
                this.Triangles[i].v0.Temp += vector;
                this.Triangles[i].v1.Temp += vector;
                this.Triangles[i].v2.Temp += vector;

                this.Triangles[i].v0.NumTrisConnected++;
                this.Triangles[i].v1.NumTrisConnected++;
                this.Triangles[i].v2.NumTrisConnected++;
            }

            for (int i = 0; i < this.NumVertexes; i++)
            {
                this.Vertexes[i].Position = this.Vertexes[i].Temp * this.divLut[this.Vertexes[i].NumTrisConnected];
            }
        }


        private void UpdateNormals()
        {
            for (int i = 0; i < this.NumTriangles; i++)
            {
                MarchingCubesTriangle t = this.Triangles[i];

                Vector3 normal = Vector3.Cross(t.v1.Position - t.v0.Position, t.v2.Position - t.v0.Position);

                this.Triangles[i].v0.Normal += normal;
                this.Triangles[i].v1.Normal += normal;
                this.Triangles[i].v2.Normal += normal;
            }

            for (int i = 0; i < this.NumVertexes; i++)
            {
                this.Vertexes[i].Normal.Normalize();
            }
        }

        public void GenerateMesh(Fluid fluid)
        {
            this.ComputeGrids(fluid);

            this.PolygonizeCubes();

            if (this.SMOOTH)
            {
                this.SmoothMesh();
            }

            this.UpdateNormals();
        }
    }
}
