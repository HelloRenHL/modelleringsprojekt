using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FluidSimulation1.Engine;
using Microsoft.Xna.Framework.Content;
using FluidSimulation1.Sim;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace FluidSimulation1
{
    public class GlassBoxScene
    {
        public Fluid Fluid;

        Slider viscositySlider;
        Slider surfaceTensionSlider;
        Slider massSlider;
        Slider particlesSlider;
        Slider timestepSlider;
        Slider rotationSpeedSlider;

        StackPanel guiElements;

        Button restartButton;
        public bool DisplayDebug = true;
        DebugGrid debugGrid;

        ContentManager content;

        FluidSimulation1 fluidSimulation1;

        float rotationSpeed = 0;
        float timestep = 1.0f;

        FluidRendererMarchingCubes fluidRenderer;
        private bool drawUsingMarchingCubes = false;

        BasicEffect marchingCubesEffect;

        public GlassBoxScene(FluidSimulation1 fluidSimulation)
        {
            this.fluidSimulation1 = fluidSimulation;
        }

        public void LoadContent()
        {
            if (content == null)
            {
                content = new ContentManager(fluidSimulation1.Services, "Content");
            }

            GlassBox glassBox = new GlassBox();
            glassBox.LoadContent(content);

            Fluid = new Fluid(2000, glassBox);
            Fluid.ActiveParticles = 500;

            surfaceTensionSlider = new Slider("Surface Tension: ", 0, 100, Fluid.SurfaceTension, 2);
            surfaceTensionSlider.OnValueChanged += new EventHandler(surfaceTensionSlider_OnValueChanged);
            surfaceTensionSlider.Length = 200;

            viscositySlider = new Slider("Viscosity: ", 0, 100, Fluid.Viscosity, 2);
            viscositySlider.OnValueChanged += new EventHandler(viscositySlider_OnValueChanged);
            viscositySlider.Length = 200;

            massSlider = new Slider("Particle Mass: ", 1, 20, Fluid.ParticleMass, 2);
            massSlider.OnValueChanged += new EventHandler(massSlider_OnValueChanged);
            massSlider.Length = 200;

            particlesSlider = new Slider("Particles: ", 1, Fluid.MaxParticles, Fluid.ActiveParticles, 0);
            particlesSlider.OnValueChanged += new EventHandler(particlesSlider_OnValueChanged);
            particlesSlider.Length = 200;

            timestepSlider = new Slider("Timestep: ", 1, 5, 1, 2);
            timestepSlider.OnValueChanged += new EventHandler(timestepSlider_OnValueChanged);
            timestepSlider.Length = 200;

            rotationSpeedSlider = new Slider("Rotation speed: ", -0.025f, 0.025f, rotationSpeed, 3);
            rotationSpeedSlider.OnValueChanged += new EventHandler(rotationSpeedSlider_OnValueChanged);
            rotationSpeedSlider.Length = 200;

            guiElements = new StackPanel(40);
            guiElements.Add(surfaceTensionSlider);
            guiElements.Add(viscositySlider);
            guiElements.Add(massSlider);
            guiElements.Add(particlesSlider);
            guiElements.Add(timestepSlider);
            guiElements.Add(rotationSpeedSlider);

            restartButton = new Button("Restart", FluidSimulation1.Font);
            restartButton.OnClick += new EventHandler(restartButton_OnClick);
            guiElements.Add(restartButton);

            debugGrid = new DebugGrid(fluidSimulation1.GraphicsDevice);

            guiElements.Font = FluidSimulation1.Font;
            guiElements.LoadContent(content);

            fluidRenderer = new FluidRendererMarchingCubes(Fluid, fluidSimulation1.GraphicsDevice);

            marchingCubesEffect = new BasicEffect(fluidSimulation1.GraphicsDevice);
        }

        public void HandleInput(InputHandler inputHandler)
        {
            if (DisplayDebug)
            {
                guiElements.HandleInput(inputHandler);
            }

            if (inputHandler.IsKeyPressed(Keys.LeftShift))
            {
                drawUsingMarchingCubes = !drawUsingMarchingCubes;
            }

            if (inputHandler.IsKeyPressed(Keys.F1))
            {
                DisplayDebug = !DisplayDebug;
            }

            if (inputHandler.IsKeyPressed(Keys.Tab))
            {
                debugGrid.Visible = !debugGrid.Visible;
            }

        }

        #region Slider Event Handlers
        void rotationSpeedSlider_OnValueChanged(object sender, EventArgs e)
        {
            rotationSpeed = rotationSpeedSlider.Value;
        }

        void timestepSlider_OnValueChanged(object sender, EventArgs e)
        {
            timestep = timestepSlider.Value;
        }

        void restartButton_OnClick(object sender, EventArgs e)
        {
            Fluid.InitializeParticles();
            Fluid.Container.Reset();
            FluidSimulation1.Camera.Reset();
            rotationSpeed = rotationSpeedSlider.Value = 0;
        }

        void particlesSlider_OnValueChanged(object sender, EventArgs e)
        {
            Fluid.ActiveParticles = (int)particlesSlider.Value;
        }

        void massSlider_OnValueChanged(object sender, EventArgs e)
        {
            Fluid.ParticleMass = massSlider.Value;
        }

        void viscositySlider_OnValueChanged(object sender, EventArgs e)
        {
            Fluid.Viscosity = viscositySlider.Value;
        }

        void surfaceTensionSlider_OnValueChanged(object sender, EventArgs e)
        {
            Fluid.SurfaceTension = surfaceTensionSlider.Value;
        }
        #endregion

        public void UnloadContent()
        {

        }

        public void Update(GameTime gameTime)
        {
            Fluid.Container.Rotate(rotationSpeed);
            Fluid.Update((float)gameTime.ElapsedGameTime.TotalSeconds, timestep);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            fluidSimulation1.ResetRenderStates();

            if (debugGrid.Visible)
            {
                debugGrid.Draw(FluidSimulation1.Camera.View, FluidSimulation1.Camera.Projection);
            }

            VertexPositionColor b = new VertexPositionColor(Vector3.Zero, Color.LightGreen);
            Vector3 temp = Fluid.Gravity;
            temp.Normalize();
            VertexPositionColor a = new VertexPositionColor(Vector3.Transform(temp, Fluid.Container.World), Color.LightSalmon);

            fluidSimulation1.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[] { a, b }, 0, 1);

            if (drawUsingMarchingCubes)
            {
                fluidRenderer.Draw(FluidSimulation1.Camera, marchingCubesEffect);
            }
            else
            {
                for (int i = 0; i < Fluid.ActiveParticles; i++)
                {
                    //Apply in SRT order (Scale * Rotation * Translation)
                    Matrix particleWorld = Matrix.CreateScale(0.8f) * Matrix.CreateTranslation(Fluid.Particles[i].Position);
                    fluidSimulation1.DrawModel(FluidSimulation1.Sphere, particleWorld, Fluid.Particles[i].Color, 1.0f);
                }
            }

            fluidSimulation1.ResetRenderStates();

            fluidSimulation1.DrawModel(Fluid.Container.Model, Fluid.Container.World, Vector3.One, Fluid.Container.Alpha);

            if (DisplayDebug)
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(FluidSimulation1.Font, "StopWatch: " + Fluid.StopWatch.ElapsedMilliseconds + "ms", new Vector2(20, 40) + Vector2.One, Color.Black);
                spriteBatch.DrawString(FluidSimulation1.Font, "StopWatch: " + Fluid.StopWatch.ElapsedMilliseconds + "ms", new Vector2(20, 40), Color.White);

                spriteBatch.DrawString(FluidSimulation1.Font, "Particles: " + Fluid.ActiveParticles, new Vector2(20, 60) + Vector2.One, Color.Black);
                spriteBatch.DrawString(FluidSimulation1.Font, "Particles: " + Fluid.ActiveParticles, new Vector2(20, 60), Color.White);

                spriteBatch.DrawString(FluidSimulation1.Font, "Camera Position: " + FluidSimulation1.Camera.Position.ToString(), new Vector2(20, 140) + Vector2.One, Color.Black);
                spriteBatch.DrawString(FluidSimulation1.Font, "Camera Position: " + FluidSimulation1.Camera.Position.ToString(), new Vector2(20, 140), Color.White);

                spriteBatch.DrawString(FluidSimulation1.Font, "Press F1 to toggle debug", new Vector2(20, 160), Color.White);
                spriteBatch.DrawString(FluidSimulation1.Font, "Press F6 to toggle fullscreen", new Vector2(20, 180), Color.White);
                spriteBatch.DrawString(FluidSimulation1.Font, "Press TAB to toggle grid", new Vector2(20, 220), Color.White);

                guiElements.Draw(spriteBatch, new Vector2(fluidSimulation1.GraphicsDevice.Viewport.Width - 220, 20));

                spriteBatch.End();
            }
        }
    }
}
