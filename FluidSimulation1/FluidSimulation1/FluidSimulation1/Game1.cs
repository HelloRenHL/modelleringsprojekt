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
        static Camera camera;

        Texture2D texture;
        SpriteFont verdana;
        Model sphere, teapot;

        float fps = 0;
        int frames = 0;
        float elapsedTime = 0;

        InputHandler inputHandler = new InputHandler();

        public static Random Random = new Random();

        FpsComponent fpsCounter;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            myFluid = new Fluid(500);
            camera = new Camera();

            camera.AspectRatio = graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight;

            float cameraDistance = 2.0f;

            camera.Position = new Vector3(0, cameraDistance, cameraDistance);

            fpsCounter = new FpsComponent(this);

            Components.Add(fpsCounter);

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
            sphere = Content.Load<Model>(@"models\smaller_sphere");
            teapot = Content.Load<Model>(@"models\teapot");
            texture = Content.Load<Texture2D>("ploj");
            verdana = Content.Load<SpriteFont>("verdana");
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

            inputHandler.Update();

            HandleInput();

            fps = frames / elapsedTime;

            camera.Update();

            myFluid.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            if (inputHandler.IsKeyPressed(Keys.R))
            {
                myFluid.InitializeParticles();
            }

            float cameraSpeed = 0.01f;

            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                camera.Position.Z -= cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                camera.Position.Z += cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                camera.Position.X += cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                camera.Position.X -= cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                camera.Position.Y += cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                camera.Position.Y -= cameraSpeed;
            }

            if (inputHandler.IsKeyPressed(Keys.D))
            {
                myFluid.Gravity = Vector3.Transform(myFluid.Gravity, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));
            }

            if (inputHandler.IsKeyPressed(Keys.A))
            {
                myFluid.Gravity = Vector3.Transform(myFluid.Gravity, Matrix.CreateRotationZ(MathHelper.ToRadians(-90)));
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            ResetRenderStates();

            foreach (FluidParticle p in myFluid.Particles)
            {
                DrawModel(sphere, Matrix.CreateTranslation(p.Position));
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(verdana, "StopWatch: " + myFluid.StopWatch.ElapsedMilliseconds, new Vector2(20, 40) + Vector2.One, Color.Black);
            spriteBatch.DrawString(verdana, "StopWatch: " + myFluid.StopWatch.ElapsedMilliseconds, new Vector2(20, 40), Color.White);

            spriteBatch.DrawString(verdana, "Particles: " + myFluid.ActiveParticles, new Vector2(20, 60) + Vector2.One, Color.Black);
            spriteBatch.DrawString(verdana, "Particles: " + myFluid.ActiveParticles, new Vector2(20, 60), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ResetRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are taken from the camera object.
        /// </summary>        
        private void DrawModel(Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}
