using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public float Fov = 90;

        public Matrix Projection;
        public Matrix View;
        public Matrix Rotation = Matrix.Identity;

        public float AspectRatio = 4 / 3;
        public float NearPlane = 0.01f;
        public float FarPlane = 100f;

        public Camera()
        {

        }

        private void BuildProjectionMatrix()
        {
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Fov), AspectRatio, NearPlane, FarPlane);
        }

        public void BuildViewMatrix()
        {
            Rotation.Translation = Position;
            View = Matrix.Invert(Rotation);
        }

        public void Update()
        {
            //HandleInput();
            BuildViewMatrix();
            BuildProjectionMatrix();
        }
    }
}
