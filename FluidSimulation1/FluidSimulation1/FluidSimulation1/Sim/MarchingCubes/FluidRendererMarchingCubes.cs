using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FluidSimulation1
{
    public class FluidRendererMarchingCubes
    {
        public static float MinimumParticleSize = 0.2f;

        private Fluid Fluid;
        private GraphicsDevice graphicsDevice;
        private bool isActive = true;
        private VertexPositionNormalTexture[] localVertices;
        private MarchingCubesField marchingCubesField;
        private int NumTriangles;

        private const int MAX_TRIANGLES = 65536; //Max number of primitives that can be drawn each update

        public FluidRendererMarchingCubes(Fluid fluid, GraphicsDevice graphicsDevice)
        {
            this.Fluid = fluid;
            this.graphicsDevice = graphicsDevice;

            // Create vertexbuffer
            this.localVertices = new VertexPositionNormalTexture[MAX_TRIANGLES * 3];
            for (int i = 0; i < localVertices.Length; i++)
            {
                this.localVertices[i] = new VertexPositionNormalTexture();
            }


            BuildField();
        }

        public void CreateMesh()
        {
            this.marchingCubesField.GenerateMesh(this.Fluid);

            //Copy mesh data into vertexbuffer
            int index = 0;
            this.NumTriangles = Math.Min(MAX_TRIANGLES, this.marchingCubesField.NumTriangles);

            for (int i = 0; i < this.NumTriangles; i++)
            {
                MarchingCubesTriangle triangle = this.marchingCubesField.Triangles[i];

                this.localVertices[index].Position = triangle.v0.Position;
                this.localVertices[index].Normal = triangle.v0.Normal;

                this.localVertices[index + 1].Position = triangle.v1.Position;
                this.localVertices[index + 1].Normal = triangle.v1.Normal;

                this.localVertices[index + 2].Position = triangle.v2.Position;
                this.localVertices[index + 2].Normal = triangle.v2.Normal;

                index += 3;
            }
        }

        public void BuildField()
        {
            float h = SmoothKernel.h;
            float delta = 0.05f; //h / 2f; // default

            float bx = this.Fluid.Container.Bounds.Min.X - h - delta;
            float by = this.Fluid.Container.Bounds.Min.Y - h - delta;
            float bz = this.Fluid.Container.Bounds.Min.Z - h - delta;

            int x = (int)Math.Ceiling((this.Fluid.Container.Bounds.Max.X - this.Fluid.Container.Bounds.Min.X + 2 * h + 2 * delta) / delta);
            int y = (int)Math.Ceiling((this.Fluid.Container.Bounds.Max.Y - this.Fluid.Container.Bounds.Min.Y + 2 * h + 2 * delta) / delta);
            int z = (int)Math.Ceiling((this.Fluid.Container.Bounds.Max.Z - this.Fluid.Container.Bounds.Min.Z + 2 * h + 2 * delta) / delta);

            this.marchingCubesField = new MarchingCubesField(x, y, z, bx, by, bz, delta, delta, delta);
        }


        public void Draw(Camera camera, Matrix world, BasicEffect basicEffect)
        {
            if (this.isActive)
            {
                this.CreateMesh();

                if (this.NumTriangles > 0)
                {
                    basicEffect.Alpha = 1.0f;
                    basicEffect.DiffuseColor = new Vector3(0.7f, 0.1f, 0);

                    basicEffect.World = world;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    basicEffect.EnableDefaultLighting();

                    // reset render states
                    graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        this.graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.localVertices, 0, this.NumTriangles);
                    }
                }
            }
        }
    }
}
