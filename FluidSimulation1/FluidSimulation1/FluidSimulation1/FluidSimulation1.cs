using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using FluidSimulation1.Engine;
using FluidSimulation1.Sim;

namespace FluidSimulation1
{
    public class FluidSimulation1 : Game
    {
        GraphicsDeviceManager graphicsDeviceManager;
        SpriteBatch spriteBatch;
        FpsComponent fpsCounter;
        InputHandler inputHandler = new InputHandler();

        public static Camera Camera;
        public static SpriteFont Font;
        public static Model Sphere;
        public static Random Random = new Random();

        Texture2D texture;
        Model teapot;
        BasicEffect lineEffect;

        GlassBoxScene glassBoxScene;

        public FluidSimulation1()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fpsCounter = new FpsComponent(this);

            graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 1024;
            //graphics.PreferMultiSampling = true;
            graphicsDeviceManager.ApplyChanges();

            Camera = new Camera();
            Camera.AspectRatio = graphicsDeviceManager.PreferredBackBufferWidth / (float)graphicsDeviceManager.PreferredBackBufferHeight;

            Camera.Reset();

            Components.Add(fpsCounter);

            IsMouseVisible = true;
        }

        #region Content Load/Unload and Initialize

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Sphere = Content.Load<Model>(@"models\smaller_sphere");
            teapot = Content.Load<Model>(@"models\teapot_small");
            texture = Content.Load<Texture2D>("ploj");
            Font = Content.Load<SpriteFont>("verdana");

            lineEffect = new BasicEffect(GraphicsDevice);

            glassBoxScene = new GlassBoxScene(this);
            glassBoxScene.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        #endregion

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
            Camera.Update();

            glassBoxScene.HandleInput(inputHandler);
            glassBoxScene.Update(gameTime);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            float cameraSpeed = 0.01f;

            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                Camera.Position.Z -= cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                Camera.Position.Z += cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                Camera.Position.X += cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                Camera.Position.X -= cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                Camera.Position.Y -= cameraSpeed;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.E))
            {
                Camera.Position.Y += cameraSpeed;
            }

            if (inputHandler.IsKeyPressed(Keys.Escape))
            {
                this.Exit();
            }

            if (inputHandler.IsKeyPressed(Keys.F6))
            {
                this.graphicsDeviceManager.IsFullScreen = !this.graphicsDeviceManager.IsFullScreen;
                this.graphicsDeviceManager.ApplyChanges();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.2f, 0.2f, 0.2f));

            glassBoxScene.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        #region Helpers

        public void ResetRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public void DrawModel(Model model, Matrix world, Vector3 diffuseColor, float alpha)
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

                    effect.Alpha = alpha; //0.33f;

                    // Use the matrices provided by the chase camera
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}
