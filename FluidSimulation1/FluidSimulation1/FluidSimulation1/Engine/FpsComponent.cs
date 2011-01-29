using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FluidSimulation1
{
    public class FpsComponent : DrawableGameComponent
    {
        int frames = 0;
        float elapsedTime;
        float fps = 0.0f;

        SpriteFont font;
        ContentManager content;
        SpriteBatch spriteBatch;

        public FpsComponent(Game1 game) : base(game)
        {

        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if(content == null)
                content = new ContentManager(Game.Services, "Content");

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            font = content.Load<SpriteFont>(@"verdana");
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            frames++;

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FPS : " + fps.ToString(), new Vector2(20, 20) + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, "FPS : " + fps.ToString(), new Vector2(20, 20), Color.White);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime >= 1.0f)
            {
                elapsedTime -= 1.0f;
                fps = frames;
                frames = 0;
            }
        }
    }
}
