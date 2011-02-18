using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public abstract class GuiElement
    {
        public SpriteFont Font;

        public bool Enabled = true;
        public bool Active = false;

        public int Height = 0;

        public virtual void HandleInput(InputHandler input)
        {
            if (!Enabled)
            {
                Active = false;
                return;
            }
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Draw(SpriteBatch spriteBatch, Vector2 position);
    }
}
