using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FluidSimulation1.Engine
{
    public class DebugGrid
    {
        BasicEffect effect;
        GraphicsDevice _graphics;
        VertexPositionColor[] verts;

        float resolution = 0.2f;
        int scale = 25;

        Color gridColor = Color.Orange;

        public bool Visible = true;

        public DebugGrid(GraphicsDevice graphics)
        {
            _graphics = graphics;

            effect = new BasicEffect(_graphics);
            effect.VertexColorEnabled = true;
            effect.World = Matrix.Identity;

            CreateGrid();
        }

        /// <summary>
        /// Creates a grid in the XZ-plane
        /// </summary>
        void CreateGrid()
        {
            if(scale % 2 != 0) // Make sure that scale is an even number
            {
                scale++;
            }

            int vertexCount = scale * 4 + 4;

            verts = new VertexPositionColor[vertexCount];

            int halfScale = scale / 2;

            float offsetY = -0.7f;

            Vector3 startX = Vector3.UnitY * offsetY +  new Vector3(-halfScale, 0, -halfScale) * resolution;
            Vector3 endX = Vector3.UnitY * offsetY + new Vector3(halfScale, 0, -halfScale) * resolution;

            Vector3 startZ = startX;
            Vector3 endZ = Vector3.UnitY * offsetY + new Vector3(-halfScale, 0, halfScale) * resolution;

            // create x-lines
            for (int i = 0; i < vertexCount; )
            {
                verts[i++] = new VertexPositionColor(startX, gridColor);
                verts[i++] = new VertexPositionColor(endX, gridColor);

                verts[i++] = new VertexPositionColor(startZ, gridColor);
                verts[i++] = new VertexPositionColor(endZ, gridColor);

                startX += Vector3.UnitZ * resolution;
                endX += Vector3.UnitZ * resolution;

                startZ += Vector3.UnitX * resolution;
                endZ += Vector3.UnitX * resolution;
            }
        }

        /// <summary>
        /// Draws the shapes that were added to the renderer and are still alive.
        /// </summary>
        /// <param name="gameTime">The current game timestamp.</param>
        /// <param name="view">The view matrix to use when rendering the shapes.</param>
        /// <param name="projection">The projection matrix to use when rendering the shapes.</param>
        public void Draw(Matrix view, Matrix projection)
        {
            // Update our effect with the matrices.
            effect.View = view;
            effect.Projection = projection;

            if (verts.Length > 0)
            {
                // Start our effect to begin rendering.
                effect.CurrentTechnique.Passes[0].Apply();

                int vertexOffset = 0;
                int lineCount = verts.Length / 2;
                while (lineCount > 0)
                {
                    // Figure out how many lines we're going to draw
                    int linesToDraw = Math.Min(lineCount, 65535);

                    // Draw the lines
                    _graphics.DrawUserPrimitives(PrimitiveType.LineList, verts, vertexOffset, linesToDraw);

                    // Move our vertex offset ahead based on the lines we drew
                    vertexOffset += linesToDraw * 2;

                    // Remove these lines from our total line count
                    lineCount -= linesToDraw;
                }
            }
        }
    }
}
