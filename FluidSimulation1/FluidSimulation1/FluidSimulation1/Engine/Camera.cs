using Microsoft.Xna.Framework;

namespace FluidSimulation1
{
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public float FieldOfView = MathHelper.ToRadians(90);

        public Matrix Projection;
        public Matrix View;
        public Matrix Rotation = Matrix.Identity;

        public float AspectRatio = 4 / 3;
        public float NearPlaneDistance = 0.01f;
        public float FarPlaneDistance = 10000f;

        public Vector3 LookAt = Vector3.Zero;

        public Camera()
        {

        }

        public void Reset()
        {
            Position = new Vector3(0, 2.0f * 0.2f, 2.0f);
        }

        /// <summary>
        /// Rebuilds camera's view and projection matricies.
        /// </summary>
        private void UpdateMatrices()
        {
            View = Matrix.CreateLookAt(this.Position, this.LookAt, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
                AspectRatio, NearPlaneDistance, FarPlaneDistance);
        }

        public void Update()
        {
            UpdateMatrices();
        }
    }
}
