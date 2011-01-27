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

        Vector3 gravity = new Vector3(0, -9.82f, 0) * 0.5f;
        List<ModelObject> teapots = new List<ModelObject>();

        InputHandler inputHandler = new InputHandler();

        const int NUMBER_OF_TEAPOTS = 100;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            myFluid = new Fluid(200);
            camera = new Camera();

            camera.Position = new Vector3(0, 100.0f, 2500.0f);

            IsMouseVisible = true;
        }

        Random r = new Random();

        public void InitializeTeapots()
        {

            teapots.Clear();

            int min = -100;
            int max = 100;

            for (int i = 0; i < NUMBER_OF_TEAPOTS; i++)
            {
                teapots.Add(new ModelObject(teapot) { Position = new Vector3(r.Next(min, max), 0, r.Next(min, max)), Acceleration = gravity, Velocity = new Vector3(r.Next(min, max), r.Next(min, max), r.Next(min, max)) * 0.66f });

            }
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


            InitializeTeapots();

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


            inputHandler.Update();

            HandleInput();

            fps = frames / elapsedTime;

            //Fluid.Update(gameTime);

            camera.Update();

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            if (inputHandler.IsKeyPressed(Keys.R))
            {
                InitializeTeapots();
            }

            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                camera.Position.Z -= 10.0f;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                camera.Position.Z += 10.0f;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                camera.Position.X += 5.0f;
            }
            if (inputHandler.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                camera.Position.X -= 5.0f;
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            // TODO: Add your drawing code here
            //Fluid.Draw(gameTime);

            //spriteBatch.Begin();
            //spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(verdana, fps.ToString(), Vector2.Zero, Color.Black);
            //spriteBatch.End();
            //DrawModel(teapot);



            foreach (ModelObject mo in teapots)
            {
                DrawModel(mo.Model, mo.World);
            }

            base.Draw(gameTime);
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
