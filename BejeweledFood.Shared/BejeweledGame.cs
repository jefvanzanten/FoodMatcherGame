using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BejeweledFood.Shared
{
    public class BejeweledGame : Game
    {
        protected GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteLoader _spriteLoader;
        private SpriteFont _debugFont;

        protected Grid _grid;

        public BejeweledGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Graphics settings should be configured in platform-specific classes
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteLoader = new SpriteLoader(Content);

            // Load assets
            Texture2D foodSpriteSheet = _spriteLoader.Load("spritesheet_food_bejeweled");
            _debugFont = Content.Load<SpriteFont>("DebugFont");

            // Calculate grid size based on screen dimensions
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;
            int rows = 8;
            int cols = 8;

            // Use the smaller dimension to ensure grid fits on screen
            int gridSize = Math.Min(screenWidth, screenHeight);
            int cellSize = gridSize / cols;

            // Center grid both horizontally and vertically
            Vector2 gridPosition = new Vector2(
                (screenWidth - gridSize) / 2,
                (screenHeight - gridSize) / 2);

            _grid = new Grid(gridPosition, cellSize, rows, cols, foodSpriteSheet, GraphicsDevice, _debugFont);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _grid.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
