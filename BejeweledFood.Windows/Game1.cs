using BejeweledFood.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BejeweledFood.Windows
{
    public class Game1 : BejeweledGame
    {
        private MouseState _previousMouseState;

        protected override void Initialize()
        {
            // Windows: 1280x720, not fullscreen
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Handle mouse input (Windows)
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePos = new Vector2(currentMouseState.X, currentMouseState.Y);

                if (_grid.IsInGrid(mousePos))
                {
                    _grid.ToggleSelection(mousePos);
                }
            }

            _previousMouseState = currentMouseState;
        }
    }
}
