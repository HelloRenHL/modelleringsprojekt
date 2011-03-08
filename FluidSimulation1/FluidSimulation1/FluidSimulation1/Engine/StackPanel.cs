using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FluidSimulation1.Engine
{
    public class StackPanel : GuiElement
    {
        public List<GuiElement> Items = new List<GuiElement>();

        public float Margin = 20.0f;

        public StackPanel()
        {
        }

        public StackPanel(int margin)
        {
            Margin = margin;
        }

        public void Add(params GuiElement[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                Items.Add(elements[i]);
            }
        }

        public override void HandleInput(InputHandler input)
        {
            base.HandleInput(input);

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].HandleInput(input);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].LoadContent(content);
                Items[i].Font = this.Font;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Draw(spriteBatch, position);
                position.Y += Items[i].Height + Margin;
            }
        }
    }
}
