using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FluidSimulation1
{
    public class Slider : GuiElement
    {
        public int Length = 100;
        public int Min;
        public int Max;
        public float Value;
        Texture2D sliderHandle;
        Texture2D sliderBackground;
        public event EventHandler OnValueChanged;
        public int Height = 20;

        public string Label = "";

        Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Length, Height);
            }
        }

        public Slider(string label, int min, int max, float initialValue)
        {
            Label = label;
            Min = min;
            Max = max;
            Value = (int)MathHelper.Clamp(initialValue, min, max);
        }

        public override void HandleInput(InputHandler input)
        {
            base.HandleInput(input);

            if(input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                Active = false;
            }

            if (Rectangle.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
            {
                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
                {
                    Active = true;
                }
            }

            if (Active)
            {
                float oldValue = Value;
                float temp = (input.CurrentMouseState.X - Position.X) / Length * Max + Min;
                Value = (float)MathHelper.Clamp(temp, Min, Max);

                if (oldValue != Value)
                {
                    if (OnValueChanged != null)
                    {
                        OnValueChanged(this, null);
                    }
                }
            }

        }

        public override void LoadContent(ContentManager content)
        {
            sliderHandle = content.Load<Texture2D>(@"slider_handle");
            sliderBackground = content.Load<Texture2D>(@"slider_background");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color backgroundColor = Color.White;
            if (Active)
            {
                backgroundColor = Color.Yellow;
            }
            spriteBatch.Draw(sliderBackground, Rectangle, backgroundColor);
            Vector2 labelPosition = Position - new Vector2(Font.MeasureString(Label).X, 0);
            spriteBatch.DrawString(Font, Label, labelPosition + Vector2.One, Color.Black);
            spriteBatch.DrawString(Font, Label, labelPosition, Color.White);

            Vector2 handlePosition = Position + new Vector2((Value - Min) / (float)Max * Length, 0);
            spriteBatch.Draw(sliderHandle, handlePosition, Color.White);

            if (Active)
            {
                Vector2 hintPosition = handlePosition + new Vector2((sliderHandle.Width - Font.MeasureString(Value.ToString()).X) * 0.5f, -Height);
                spriteBatch.DrawString(Font, Value.ToString(), hintPosition + Vector2.One, Color.Black);
                spriteBatch.DrawString(Font, Value.ToString(), hintPosition, Color.White);
            }
        }
    }
}
