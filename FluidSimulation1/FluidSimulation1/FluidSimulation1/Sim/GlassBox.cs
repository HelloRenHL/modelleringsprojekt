using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FluidSimulation1.Sim
{
    public class GlassBox
    {
        public Vector3 Position = Vector3.Zero;

        public float Width = 1;
        public float Height = 1;
        public float Depth = 1;

        public Model Model;

        public Vector3 Forward = Vector3.Forward;
        public Vector3 Up = Vector3.Up;
        public Vector3 Right = Vector3.Right;

        public Matrix World
        {
            get
            {
                // Construct a matrix to transform from object space to worldspace
                Matrix temp = Matrix.Identity;
                temp.Forward = Forward;
                temp.Up = Up;
                temp.Right = Right;
                temp.Translation = Position;

                return Matrix.CreateScale(Width, Height, Depth) * temp;
            }
        }

        public GlassBox(float width, float height, float depth, Vector3 position)
        {
            Position = position;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public void LoadContent(ContentManager content)
        {
            Model = content.Load<Model>(@"models\glass_box1");
        }

        public void Rotate(float amount)
        {
            //Matrix rotationMatrix = Matrix.CreateFromAxisAngle(Right, amount);

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(0, 0, amount);

            Right = Vector3.TransformNormal(Right, rotationMatrix);
            //Up = Vector3.TransformNormal(Up, rotationMatrix);

            Right.Normalize();
            Up.Normalize();

            //re-calculate right
            Forward = Vector3.Cross(Up, Right);

            //re-calculate up to maintain orthogonality
            Up = Vector3.Cross(Right, Forward);
        }
    }
}
