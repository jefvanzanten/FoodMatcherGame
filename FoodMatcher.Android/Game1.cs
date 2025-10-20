using BejeweledFood.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace BejeweledFood.Android
{
    public class Game1 : BejeweledGame
    {
        private bool _previousTouchPressed;

        public Game1()
        {
            // Enable touch input for Android
            TouchPanel.EnabledGestures = GestureType.Tap;
        }

        protected override void Initialize()
        {
            // Android: fullscreen
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Handle touch input (Android)
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                TouchLocation touch = touches[0];

                // Detect tap (press)
                if (touch.State == TouchLocationState.Pressed && !_previousTouchPressed)
                {
                    Vector2 touchPos = touch.Position;

                    if (_grid.IsInGrid(touchPos))
                    {
                        _grid.ToggleSelection(touchPos);
                    }
                }

                _previousTouchPressed = (touch.State == TouchLocationState.Pressed);
            }
            else
            {
                _previousTouchPressed = false;
            }
        }
    }
}
