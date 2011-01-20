using Microsoft.Xna.Framework.Input;

namespace FluidSimulation1
{
    public class InputHandler
    {
        public KeyboardState CurrentKeyboardState;
        public KeyboardState LastKeyboardState;

        public MouseState CurrentMouseState;
        public MouseState LastMouseState;

        public bool MenuUp
        {
            get
            {
                return IsKeyPressed(Keys.Up);
            }
        }

        public bool MenuDown
        {
            get
            {
                return IsKeyPressed(Keys.Down);
            }
        }

        public bool MenuEnter
        {
            get
            {
                return IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space);
            }
        }

        public bool MenuCancel
        {
            get
            {
                return IsKeyPressed(Keys.Escape);
            }
        }

        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
        }
    }
}
