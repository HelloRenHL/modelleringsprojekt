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
        private Fluid Fluid;
        private GraphicsDevice graphicsDevice;
        private bool isActive = true;
        private VertexPositionNormalTexture[] localVertices;
        private MarchingCubesField marchingCubesField;
        private float MinParticleSize = 0.2f;
        private int NumTriangles;

        private VertexBuffer vertexBuffer;

        private const int MAX_TRIANGLES = 65536;

        public FluidRendererMarchingCubes(Fluid fluid, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.localVertices = new VertexPositionNormalTexture[MAX_TRIANGLES * 3];
            this.vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), localVertices.Length, BufferUsage.WriteOnly | BufferUsage.None);

            this.Fluid = fluid;
            BuildField();
        }

        public void CreateMesh()
        {
            this.marchingCubesField.GenerateMesh(ref this.Fluid);
            int index = 0;
            int num2 = 1;
            int num3 = 2;
            this.NumTriangles = Math.Min(MAX_TRIANGLES, this.marchingCubesField.NumTriangles);

            for (int i = 0; i < this.NumTriangles; i++)
            {
                MarchingCubesTriangle triangle = this.marchingCubesField.Triangles[i];

                this.localVertices[index].Position = triangle.v0.Position;
                this.localVertices[num2].Position = triangle.v1.Position;
                this.localVertices[num3].Position = triangle.v2.Position;

                this.localVertices[index].Normal = triangle.v0.Normal;
                this.localVertices[num2].Normal = triangle.v1.Normal;
                this.localVertices[num3].Normal = triangle.v2.Normal;

                index += 3;
                num2 += 3;
                num3 += 3;
            }
        }

        public void BuildField()
        {
            float num = 0.15f;
            float num2 = 0.06f; // default
            float num3 = (this.Fluid.Container.Bounds.Min.X - num) - num2;
            float num4 = (this.Fluid.Container.Bounds.Min.Y - num) - num2;
            float num5 = (this.Fluid.Container.Bounds.Min.Z - num) - num2;
            float num6 = (this.Fluid.Container.Bounds.Max.X + num) + num2;
            float num7 = (this.Fluid.Container.Bounds.Max.Y + num) + num2;
            float num8 = (this.Fluid.Container.Bounds.Max.Z + num) + num2;
            int num9 = (int)Math.Ceiling((double)((num6 - num3) / num2));
            int num10 = (int)Math.Ceiling((double)((num7 - num4) / num2));
            int num11 = (int)Math.Ceiling((double)((num8 - num5) / num2));

            this.marchingCubesField = new MarchingCubesField(num9, num10, num11, num3, num4, num5, num2, num2, num2);
            this.marchingCubesField.MinimumParticleSize = this.MinParticleSize;
        }


        public void Draw(Camera camera, BasicEffect basicEffect)
        {
            if (this.isActive)
            {
                this.CreateMesh();

                if (this.NumTriangles > 0)
                {
                    graphicsDevice.SetVertexBuffer(null); // unset vertexbuffer from graphicsdevice
                    vertexBuffer.SetData<VertexPositionNormalTexture>(localVertices); // update vertexbuffer
                    graphicsDevice.SetVertexBuffer(vertexBuffer); // set vertexbuffer

                    basicEffect.Alpha = 1.0f;
                    basicEffect.DiffuseColor = new Vector3(0.7f, 0.1f, 0);

                    basicEffect.World = Matrix.Identity;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    basicEffect.EnableDefaultLighting();

                    // reset render states
                    graphicsDevice.BlendState = BlendState.AlphaBlend;
                    graphicsDevice.DepthStencilState = DepthStencilState.Default;
                    graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        this.graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, this.NumTriangles);
                    }
                }
            }
        }

        /*public void UpdateShaders()
        {
            Matrix matrix = (Renderer.WorldMatrix * Renderer.ViewMatrix) * Renderer.ProjectionMatrix;
            Matrix matrix2 = Matrix.Transpose(Matrix.Invert(Renderer.WorldMatrix));
            Matrix matrix3 = Matrix.Transpose(Matrix.Invert(Renderer.WorldMatrix * Renderer.ViewMatrix));
            this.m_deferredShader.get_Parameters().get_Item("WorldViewProj").SetValue(matrix);
            this.m_deferredShader.get_Parameters().get_Item("WorldView").SetValue(Renderer.WorldMatrix * Renderer.ViewMatrix);
            this.m_deferredShader.get_Parameters().get_Item("WorldViewIT").SetValue(matrix3);
            this.m_deferredShader.get_Parameters().get_Item("WorldIT").SetValue(matrix2);
            this.m_deferredShader.get_Parameters().get_Item("World").SetValue(Renderer.WorldMatrix);
            this.m_deferredShader.get_Parameters().get_Item("ViewInv").SetValue(Matrix.Invert(Renderer.ViewMatrix));
            this.m_deferredShader.get_Parameters().get_Item("cubeMap").SetValue(this.m_cubemap);
        }*/
    }
}
