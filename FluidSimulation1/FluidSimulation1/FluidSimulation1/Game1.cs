using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using FluidSimulation1.Engine;

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

        InputHandler inputHandler = new InputHandler();

        public static Random Random = new Random();

        FpsComponent fpsCounter;

        Texture2D arrow;

        Slider viscositySlider;
        Slider surfaceTensionSlider;
        Slider massSlider;
        Slider particlesSlider;

        List<GuiElement> guiElements = new List<GuiElement>();

        Button restartButton;

        public bool DisplayDebug = true;

        DebugGrid debugGrid;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            myFluid = new Fluid(2000);
            myFluid.ActiveParticles = 500;

            fpsCounter = new FpsComponent(this);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            //graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            camera = new Camera();
            camera.AspectRatio = graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float cameraDistance = 2.0f;
            camera.Position = new Vector3(0, cameraDistance * 0.2f, cameraDistance);

            Components.Add(fpsCounter);

            debugGrid = new DebugGrid(camera, this);

            Components.Add(debugGrid);

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
            teapot = Content.Load<Model>(@"models\smaller_sphere");
            texture = Content.Load<Texture2D>("ploj");
            verdana = Content.Load<SpriteFont>("verdana");

            arrow = Content.Load<Texture2D>(@"arrow");

            int offset = 220;

            surfaceTensionSlider = new Slider("Surface Tension: ", 0, 100, myFluid.SurfaceTension);
            surfaceTensionSlider.Position = new Vector2(graphics.PreferredBackBufferWidth - offset, 60);
            surfaceTensionSlider.OnValueChanged += new EventHandler(surfaceTensionSlider_OnValueChanged);
            surfaceTensionSlider.Length = 200;

            viscositySlider = new Slider("Viscosity: ", 0, 100, myFluid.Viscosity);
            viscositySlider.OnValueChanged += new EventHandler(viscositySlider_OnValueChanged);
            viscositySlider.Position = new Vector2(graphics.PreferredBackBufferWidth - offset, 20);
            viscositySlider.Length = 200;

            massSlider = new Slider("Particle Mass: ", 1, 20, myFluid.ParticleMass);
            massSlider.Position = new Vector2(graphics.PreferredBackBufferWidth - offset, 100);
            massSlider.OnValueChanged += new EventHandler(massSlider_OnValueChanged);
            massSlider.Length = 200;

            particlesSlider = new Slider("Particles: ", 1, myFluid.MaxParticles, myFluid.ActiveParticles);
            particlesSlider.Position = new Vector2(graphics.PreferredBackBufferWidth - offset, 140);
            particlesSlider.OnValueChanged += new EventHandler(particlesSlider_OnValueChanged);
            particlesSlider.Length = 200;

            guiElements.Add(surfaceTensionSlider);
            guiElements.Add(viscositySlider);
            guiElements.Add(massSlider);
            guiElements.Add(particlesSlider);

            restartButton = new Button("Restart", verdana);
            restartButton.Position = new Vector2(graphics.PreferredBackBufferWidth - offset, 180);
            restartButton.OnClick += new EventHandler(restartButton_OnClick);
            guiElements.Add(restartButton);

            foreach (GuiElement element in guiElements)
            {
                element.LoadContent(Content);
                element.Font = verdana;
            }

        }

        void restartButton_OnClick(object sender, EventArgs e)
        {
            myFluid.InitializeParticles();
        }

        void particlesSlider_OnValueChanged(object sender, EventArgs e)
        {
            myFluid.ActiveParticles = particlesSlider.Value;
        }

        void massSlider_OnValueChanged(object sender, EventArgs e)
        {
            myFluid.ParticleMass = massSlider.Value;
        }

        void viscositySlider_OnValueChanged(object sender, EventArgs e)
        {
            myFluid.Viscosity = viscositySlider.Value;
        }

        void surfaceTensionSlider_OnValueChanged(object sender, EventArgs e)
        {
            myFluid.SurfaceTension = surfaceTensionSlider.Value;
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            inputHandler.Update();

            HandleInput();

            camera.Update();

            myFluid.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            if (DisplayDebug)
            {
                foreach (GuiElement element in guiElements)
                {
                    element.HandleInput(inputHandler);
                }
            }

            if (inputHandler.IsKeyPressed(Keys.F1))
            {
                DisplayDebug = !DisplayDebug;
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
                myFluid.GravityRotation += 45.0f;
            }

            if (inputHandler.IsKeyPressed(Keys.A))
            {
                myFluid.GravityRotation -= 45.0f;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.2f, 0.2f, 0.2f));

            base.Draw(gameTime);

            ResetRenderStates();

            //Apply in SRT order (Scale * Rotation * Translation)
            for (int i = 0; i < myFluid.ActiveParticles; i++)
            {
                DrawModel(sphere, Matrix.CreateScale(0.8f) * Matrix.CreateTranslation(myFluid.Particles[i].Position), myFluid.Particles[i].Color);
            }

            if (DisplayDebug)
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(verdana, "StopWatch: " + myFluid.StopWatch.ElapsedMilliseconds + "ms", new Vector2(20, 40) + Vector2.One, Color.Black);
                spriteBatch.DrawString(verdana, "StopWatch: " + myFluid.StopWatch.ElapsedMilliseconds + "ms", new Vector2(20, 40), Color.White);

                spriteBatch.DrawString(verdana, "Particles: " + myFluid.ActiveParticles, new Vector2(20, 60) + Vector2.One, Color.Black);
                spriteBatch.DrawString(verdana, "Particles: " + myFluid.ActiveParticles, new Vector2(20, 60), Color.White);

                string temp = "Gravity Direction:";
                spriteBatch.DrawString(verdana, temp, new Vector2(20, 90) + Vector2.One, Color.Black);
                spriteBatch.DrawString(verdana, temp, new Vector2(20, 90), Color.White);
                spriteBatch.Draw(arrow, new Vector2(30 + verdana.MeasureString(temp).X, 90) + Vector2.One * 16.0f, null, Color.White, MathHelper.ToRadians(-myFluid.GravityRotation), Vector2.One * 16.0f, 1.0f, SpriteEffects.None, 0);

                spriteBatch.DrawString(verdana, "Camera Position: " + camera.Position.ToString(), new Vector2(20, 140) + Vector2.One, Color.Black);
                spriteBatch.DrawString(verdana, "Camera Position: " + camera.Position.ToString(), new Vector2(20, 140), Color.White);

                foreach (GuiElement element in guiElements)
                {
                    element.Draw(spriteBatch);
                }

                spriteBatch.End();
            }
        }

        private void ResetRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawModel(Model model, Matrix world, Vector3 diffuseColor)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = diffuseColor;
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
