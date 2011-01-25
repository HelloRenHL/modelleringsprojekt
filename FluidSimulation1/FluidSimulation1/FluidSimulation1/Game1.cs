using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace FluidSimulation1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static Fluid myFluid;
        public static Camera Camera;

        Texture2D texture;
        SpriteFont verdana;
        Model sphere, teapot;

        float fps = 0;
        int frames = 0;
        float elapsedTime = 0;

        Vector3 gravity = new Vector3(0, -9.82f, 0) * 0.001f;
        List<ModelObject> teapots = new List<ModelObject>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            myFluid = new Fluid(200);
            Camera = new Camera();

            aspectRatio = graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight;

            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            sphere = Content.Load<Model>(@"models\sphere");
            teapot = Content.Load<Model>(@"models\teapot");
            texture = Content.Load<Texture2D>("ploj");
            verdana = Content.Load<SpriteFont>("verdana");
            
            Random r = new Random();

            for (int i = 0; i < 100; i++)
            {
                teapots.Add(new ModelObject(teapot) { Position = new Vector3(r.Next(), 0, r.Next()) * .0000000030f, Acceleration = gravity + new Vector3(r.Next(), r.Next(), r.Next()) * 0.0000000001f, Velocity = -2*gravity });
                
            }

            Debug.WriteLine(r.Next());

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            frames++;

            foreach (ModelObject mo in teapots)
            {
                mo.Update();
            }


            fps = frames / elapsedTime;

            //Fluid.Update(gameTime);

            // TODO: Add your update logic here

            //if (Keyboard.GetState().IsKeyDown(Keys.Up))
            //{
            //    modelPosition.Y+=25;
            //}
            //else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            //{
            //    modelPosition.Y -= 25;
            //}

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                modelRotation += MathHelper.ToRadians(5);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                modelRotation-=MathHelper.ToRadians(5);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Bisque);

            // TODO: Add your drawing code here
            //Fluid.Draw(gameTime);

            //spriteBatch.Begin();
            //spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(verdana, fps.ToString(), Vector2.Zero, Color.Black);
            //spriteBatch.End();
            //DrawModel(teapot);

            foreach (ModelObject mo in teapots)
            {
                DrawModel(mo);
            }

            base.Draw(gameTime);
        }

        // Set the position of the model in world space, and set the rotation.
        //Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 500.0f);
        private float aspectRatio;

        protected void DrawModel(ModelObject myModel)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Model.Bones.Count];
            myModel.Model.CopyAbsoluteBoneTransformsTo(transforms);
            myModel.Model.Root.Transform = Matrix.CreateTranslation(myModel.Position);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel.Model.Meshes)
            {
                
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateRotationY(modelRotation);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(90.0f), aspectRatio,
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}
