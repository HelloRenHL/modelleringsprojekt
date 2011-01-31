using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace FluidSimulation1
{
    public class Button : GuiElement
    {
        public string Label;
        public int Padding = 12;

        public event EventHandler OnClick;

        private Rectangle rectangle;

        public Texture2D ButtonTexture;

        private void UpdateRectangle()
        {
            Vector2 labelSize = Font.MeasureString(Label);
            rectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)labelSize.X + 2 * Padding, (int)labelSize.Y + 2 * Padding);
        }

        public Button(string label, SpriteFont font)
        {
            Label = label;
            Font = font;
        }

        public override void LoadContent(ContentManager content)
        {
            ButtonTexture = content.Load<Texture2D>(@"button");
        }

        public override void HandleInput(InputHandler input)
        {
            base.HandleInput(input);

            bool oldActive = Active;
            Active = false;

            if(rectangle.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
            {
                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
                {
                    Active = true;
                }
                else if(oldActive == true) // Currently not pressed but active was true last update -> fire click event
                {
                    if (OnClick != null)
                    {
                        OnClick(this, null);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color backgroundColor = Color.White;
            if (Active)
            {
                backgroundColor = Color.Yellow;
            }

            UpdateRectangle();
            
            spriteBatch.Draw(ButtonTexture, rectangle, backgroundColor);
            spriteBatch.DrawString(Font, Label, Position + Vector2.One * Padding + Vector2.One, Color.Black);
            spriteBatch.DrawString(Font, Label, Position + Vector2.One * Padding, Color.White);
        }
    }
}
