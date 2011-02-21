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
        private double[] divLut;
        private float dx;
        private float dy;
        private float dz;
        private float[] m_fieldSmooth;
        public float[] m_isInside;
        public float MinimumParticleSize = 0.2f;
        public int NumTriangles;
        public int m_numVertices;
        public MarchingCubesTriangle[] Triangles;
        public MarchingCubesVertex[] m_vertex;
        private int MAX_CUBE_TRIANGLES = 5;
        private bool SMOOTH = true;
        private int X;
        private MarchingCubesVertex[] xv;
        private int Y;
        private MarchingCubesVertex[] yv;
        private int Z;
        private MarchingCubesVertex[] zv;

        public MarchingCubesField(int X_, int Y_, int Z_, float bx_, float by_, float bz_, float dx_, float dy_, float dz_)
        {
            this.X = X_;
            this.Y = Y_;
            this.Z = Z_;
            this.m_isInside = new float[(this.X * this.Y) * this.Z];
            this.m_fieldSmooth = new float[(this.X * this.Y) * this.Z];
            this.xv = new MarchingCubesVertex[((this.X - 1) * this.Y) * this.Z];
            this.yv = new MarchingCubesVertex[(this.X * (this.Y - 1)) * this.Z];
            this.zv = new MarchingCubesVertex[(this.X * this.Y) * (this.Z - 1)];
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
            this.Triangles = new MarchingCubesTriangle[((this.MAX_CUBE_TRIANGLES * (this.X - 1)) * (this.Y - 1)) * (this.Z - 1)];
            for (int m = 0; m < this.Triangles.Length; m++)
            {
                this.Triangles[m] = new MarchingCubesTriangle();
            }
            this.NumTriangles = 0;
            this.m_vertex = new MarchingCubesVertex[((((this.X - 1) * this.Y) * this.Z) + ((this.X * (this.Y - 1)) * this.Z)) + ((this.X * this.Y) * (this.Z - 1))];
            for (int n = 0; n < this.m_vertex.Length; n++)
            {
                this.m_vertex[n] = new MarchingCubesVertex();
            }
            this.m_numVertices = 0;
            this.divLut = new double[20];
            this.divLut[0] = 0.0;
            for (int num6 = 1; num6 < 20; num6++)
            {
                this.divLut[num6] = 1.0 / (num6 * 3.0);
            }
        }

        private Vector3 Bisect(float left, float right, Vector3 pleft, Vector3 pright)
        {
            float num = left / (left - right);
            return (pleft + ((Vector3)((pright - pleft) * num)));
        }

        private void ComputeGrids(ref Fluid fluid)
        {
            int num;
            int num2;
            int num3;

            fluid.CreateColorField(ref this.m_isInside, this.MinimumParticleSize, this.bx, this.by, this.bz, this.dx, this.dy, this.dz, this.X, this.Y, this.Z);
            
            for (int i = 0; i < 1; i++)
            {
                //this.SmoothColorField();
            }

            this.m_numVertices = 0;
            for (num3 = 0; num3 < this.Z; num3++)
            {
                num2 = 0;
                while (num2 < this.Y)
                {
                    num = 0;
                    while (num < (this.X - 1))
                    {
                        int index = (((num3 * this.Y) + num2) * this.X) + num;
                        int num6 = (((num3 * this.Y) + num2) * this.X) + (num + 1);
                        int num7 = (((num3 * this.Y) + num2) * (this.X - 1)) + num;
                        if ((this.m_isInside[index] * this.m_isInside[num6]) < 0f)
                        {
                            this.xv[num7].position = this.Bisect(this.m_isInside[index], this.m_isInside[num6], this.GCSToWCS((float)num, (float)num2, (float)num3), this.GCSToWCS((float)(num + 1), (float)num2, (float)num3));
                            this.xv[num7].normal = Vector3.Zero;
                            this.m_vertex[this.m_numVertices] = this.xv[num7];
                            this.m_numVertices++;
                        }
                        num++;
                    }
                    num2++;
                }
            }

            for (num3 = 0; num3 < this.Z; num3++)
            {
                num2 = 0;
                while (num2 < (this.Y - 1))
                {
                    num = 0;
                    while (num < this.X)
                    {
                        int num8 = (((num3 * this.Y) + num2) * this.X) + num;
                        int num9 = (((num3 * this.Y) + (num2 + 1)) * this.X) + num;
                        int num10 = (((num3 * (this.Y - 1)) + num2) * this.X) + num;
                        if ((this.m_isInside[num8] * this.m_isInside[num9]) < 0f)
                        {
                            this.yv[num10].position = this.Bisect(this.m_isInside[num8], this.m_isInside[num9], this.GCSToWCS((float)num, (float)num2, (float)num3), this.GCSToWCS((float)num, (float)(num2 + 1), (float)num3));
                            this.yv[num10].normal = Vector3.Zero;
                            this.m_vertex[this.m_numVertices] = this.yv[num10];
                            this.m_numVertices++;
                        }
                        num++;
                    }
                    num2++;
                }
            }

            for (num3 = 0; num3 < (this.Z - 1); num3++)
            {
                for (num2 = 0; num2 < this.Y; num2++)
                {
                    for (num = 0; num < this.X; num++)
                    {
                        int num11 = (((num3 * this.Y) + num2) * this.X) + num;
                        int num12 = ((((num3 + 1) * this.Y) + num2) * this.X) + num;
                        int num13 = (((num3 * this.Y) + num2) * this.X) + num;
                        if ((this.m_isInside[num11] * this.m_isInside[num12]) < 0f)
                        {
                            this.zv[num13].position = this.Bisect(this.m_isInside[num11], this.m_isInside[num12], this.GCSToWCS((float)num, (float)num2, (float)num3), this.GCSToWCS((float)num, (float)num2, (float)(num3 + 1)));
                            this.zv[num13].normal = Vector3.Zero;
                            this.m_vertex[this.m_numVertices] = this.zv[num13];
                            this.m_numVertices++;
                        }
                    }
                }
            }
        }

        private Vector3 GCSToWCS(float x, float y, float z)
        {
            return new Vector3(((x + 0.5f) * this.dx) + this.bx, ((y + 0.5f) * this.dy) + this.by, ((z + 0.05f) * this.dz) + this.bz);
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
            for (int i = 0; i < ((this.X * this.Y) * this.Z); i++)
            {
                this.m_fieldSmooth[i] = this.m_isInside[i];
            }
            for (int j = 0; j < this.Z; j++)
            {
                for (int k = 0; k < this.Y; k++)
                {
                    for (int m = 0; m < this.X; m++)
                    {
                        this.m_isInside[(((j * this.Y) + k) * this.X) + m] = (((((this.m_fieldSmooth[(((j * this.Y) + k) * this.X) + this.Clamp(m + 1, this.X)] + this.m_fieldSmooth[(((j * this.Y) + k) * this.X) + this.Clamp(m - 1, this.X)]) + this.m_fieldSmooth[(((j * this.Y) + this.Clamp(k + 1, this.Y)) * this.X) + m]) + this.m_fieldSmooth[(((j * this.Y) + this.Clamp(k - 1, this.Y)) * this.X) + m]) + this.m_fieldSmooth[(((this.Clamp(j + 1, this.Z) * this.Y) + k) * this.X) + m]) + this.m_fieldSmooth[(((this.Clamp(j - 1, this.Z) * this.Y) + k) * this.X) + m]) / 6f;
                    }
                }
            }
        }

        private void PolygonizeCube(int i, int j, int k, MarchingCubesVertex[] vertlist)
        {
            int index = 0;
            if (this.m_isInside[(((k * this.Y) + j) * this.X) + i] < 0f)
            {
                index |= 1;
            }
            if (this.m_isInside[(((k * this.Y) + j) * this.X) + (i + 1)] < 0f)
            {
                index |= 2;
            }
            if (this.m_isInside[((((k + 1) * this.Y) + j) * this.X) + (i + 1)] < 0f)
            {
                index |= 4;
            }
            if (this.m_isInside[((((k + 1) * this.Y) + j) * this.X) + i] < 0f)
            {
                index |= 8;
            }
            if (this.m_isInside[(((k * this.Y) + (j + 1)) * this.X) + i] < 0f)
            {
                index |= 0x10;
            }
            if (this.m_isInside[(((k * this.Y) + (j + 1)) * this.X) + (i + 1)] < 0f)
            {
                index |= 0x20;
            }
            if (this.m_isInside[((((k + 1) * this.Y) + (j + 1)) * this.X) + (i + 1)] < 0f)
            {
                index |= 0x40;
            }
            if (this.m_isInside[((((k + 1) * this.Y) + (j + 1)) * this.X) + i] < 0f)
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
            for (int i = 0; i < (this.Z - 1); i++)
            {
                for (int j = 0; j < (this.Y - 1); j++)
                {
                    for (int k = 0; k < (this.X - 1); k++)
                    {
                        this.PolygonizeCube(k, j, i, vertlist);
                    }
                }
            }
        }


        private void SmoothMesh()
        {
            int num;
            for (num = 0; num < this.m_numVertices; num++)
            {
                this.m_vertex[num].temp = Vector3.Zero;
                this.m_vertex[num].numTrisConnected = 0;
            }
            for (num = 0; num < this.NumTriangles; num++)
            {
                Vector3 vector = (this.Triangles[num].v0.position + this.Triangles[num].v1.position) + this.Triangles[num].v2.position;
                this.Triangles[num].v0.temp += vector;
                this.Triangles[num].v1.temp += vector;
                this.Triangles[num].v2.temp += vector;
                this.Triangles[num].v0.numTrisConnected++;
                this.Triangles[num].v1.numTrisConnected++;
                this.Triangles[num].v2.numTrisConnected++;
            }
            for (num = 0; num < this.m_numVertices; num++)
            {
                this.m_vertex[num].position = (Vector3)(this.m_vertex[num].temp * ((float)this.divLut[this.m_vertex[num].numTrisConnected]));
            }
        }


        private void UpdateNormals()
        {
            int num;
            for (num = 0; num < this.NumTriangles; num++)
            {
                Vector3 position = this.Triangles[num].v0.position;
                Vector3 vector2 = this.Triangles[num].v1.position;
                Vector3 vector3 = this.Triangles[num].v2.position;
                Vector3 vector4 = Vector3.Cross(vector2 - position, vector3 - position);
                this.Triangles[num].v0.normal += vector4;
                this.Triangles[num].v1.normal += vector4;
                this.Triangles[num].v2.normal += vector4;
            }
            for (num = 0; num < this.m_numVertices; num++)
            {
                this.m_vertex[num].normal.Normalize();
            }
        }

        public void GenerateMesh(ref Fluid fluid)
        {
            this.ComputeGrids(ref fluid);
            this.PolygonizeCubes();
            if (this.SMOOTH)
            {
                this.SmoothMesh();
            }
            this.UpdateNormals();
        }
    }
}
