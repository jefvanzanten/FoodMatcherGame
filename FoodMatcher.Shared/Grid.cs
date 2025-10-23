using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace BejeweledFood.Shared
{
    public class Grid
    {
        private readonly Random _random;
        private readonly Texture2D _spriteSheet;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _debugFont;
        private const int FoodTypeCount = 9;
        private const int SpriteSize = 60;

        public Vector2 WorldPos { get; set; }
        public int CellSize { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int[,] Map { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public Point? SelectedCell { get; private set; } 

        public Grid(Vector2 worldPos, int cellSize, int rows, int cols, Texture2D spriteSheet, GraphicsDevice graphicsDevice, SpriteFont debugFont)
        {
            _random = new Random();
            _spriteSheet = spriteSheet;
            _pixelTexture = CreatePixelTexture(graphicsDevice);
            _debugFont = debugFont;
            WorldPos = worldPos;
            CellSize = cellSize;
            Rows = rows;
            Cols = cols;
            GridWidth = Rows * CellSize;
            GridHeight = Cols * CellSize;
            Map = new int[Cols, Rows];
            InitializeGrid();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawSprites(spriteBatch);

            DrawGridLines(spriteBatch);

            if (SelectedCell.HasValue)
            {
                DrawSelectedHighlight(spriteBatch);
            }

            DrawDebugMap(spriteBatch);
        }

        public bool IsInGrid(Vector2 worldPos)
        {
            if(worldPos.X >= this.WorldPos.X && worldPos.X <= this.WorldPos.X + GridWidth &&
                worldPos.Y >= this.WorldPos.Y && worldPos.Y <= this.WorldPos.Y + GridHeight)
            {
                return true;
            }
            return false;
        }

        public int FoodTypeOnGridPos(Vector2 worldPos)
        {
            Point p = WorldPosToGridPos(worldPos);
            return Map[p.X, p.Y];
        }

        public void ToggleSelection(Vector2 worldPos)
        {
            Point clickedCell = WorldPosToGridPos(worldPos);

            if (SelectedCell.HasValue && SelectedCell.Value == clickedCell)
            {
                SelectedCell = null;
            }
            else if (SelectedCell.HasValue)
            {
                if (AreAdjacent(SelectedCell.Value, clickedCell))
                {
                    SwapCells(SelectedCell.Value, clickedCell);

                    if (IsAdjacentTriple(clickedCell) || IsAdjacentTriple(SelectedCell.Value))
                    {
                        Debug.WriteLine("Matching neighbours are found.");

                        // Delete matching food sprites with animation
                        DeleteMatchingSprites();
                        // update grid with animation and fill empty spots randomly
                        UpdateGridAfterMatch();
                        // Scan for triples
                        ScanForTriples();
                    }

                    else
                    {
                        SwapCells(clickedCell, SelectedCell.Value);
                    } 

                    SelectedCell = null;
                }
                else
                {
                    SelectedCell = clickedCell;
                }
            }
            else
            {
                SelectedCell = clickedCell;
            }
        }

        private void DeleteMatchingSprites()
        {
            bool[,] toDelete = new bool[Cols, Rows];

            // Scan horizontal matches
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols - 2; x++)
                {
                    int current = Map[x, y];
                    if (current == Map[x + 1, y] && current == Map[x + 2, y])
                    {
                        toDelete[x, y] = true;
                        toDelete[x + 1, y] = true;
                        toDelete[x + 2, y] = true;
                    }
                }
            }

            // Scan vertical matches
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows - 2; y++)
                {
                    int current = Map[x, y];
                    if (current == Map[x, y + 1] && current == Map[x, y + 2])
                    {
                        toDelete[x, y] = true;
                        toDelete[x, y + 1] = true;
                        toDelete[x, y + 2] = true;
                    }
                }
            }

            // Mark cells for deletion (use -1 as empty marker)
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (toDelete[x, y])
                    {
                        Map[x, y] = -1;
                    }
                }
            }
        }

        private void UpdateGridAfterMatch()
        {
            // Apply gravity: move cells down to fill empty spots
            for (int x = 0; x < Cols; x++)
            {
                int writeY = Rows - 1; // Start from bottom

                // Scan from bottom to top
                for (int readY = Rows - 1; readY >= 0; readY--)
                {
                    if (Map[x, readY] != -1)
                    {
                        // Move non-empty cell down
                        Map[x, writeY] = Map[x, readY];
                        if (writeY != readY)
                        {
                            Map[x, readY] = -1;
                        }
                        writeY--;
                    }
                }

                // Fill remaining empty spots at top with random food
                for (int y = writeY; y >= 0; y--)
                {
                    Map[x, y] = _random.Next(0, FoodTypeCount);
                }
            }
        }

        private void ScanForTriples()
        {
            bool foundMatch = false;

            // Check for any matches in the grid
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    // Check horizontal
                    if (x < Cols - 2)
                    {
                        int current = Map[x, y];
                        if (current == Map[x + 1, y] && current == Map[x + 2, y])
                        {
                            foundMatch = true;
                            break;
                        }
                    }

                    // Check vertical
                    if (y < Rows - 2)
                    {
                        int current = Map[x, y];
                        if (current == Map[x, y + 1] && current == Map[x, y + 2])
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                }
                if (foundMatch) break;
            }

            // If matches found, recursively delete and refill
            if (foundMatch)
            {
                Debug.WriteLine("New matches found after gravity!");
                DeleteMatchingSprites();
                UpdateGridAfterMatch();
                ScanForTriples(); // Recursion
            }
        }

        private bool IsAdjacentTriple(Point p)
        {
            int value = Map[p.X, p.Y];

            // Check horizontal: left-left-center, left-center-right, center-right-right
            if ((p.X >= 2 && Map[p.X - 1, p.Y] == value && Map[p.X - 2, p.Y] == value) ||
                (p.X >= 1 && p.X < Cols - 1 && Map[p.X - 1, p.Y] == value && Map[p.X + 1, p.Y] == value) ||
                (p.X < Cols - 2 && Map[p.X + 1, p.Y] == value && Map[p.X + 2, p.Y] == value))
                return true;

            // Check vertical: up-up-center, up-center-down, center-down-down
            if ((p.Y >= 2 && Map[p.X, p.Y - 1] == value && Map[p.X, p.Y - 2] == value) ||
                (p.Y >= 1 && p.Y < Rows - 1 && Map[p.X, p.Y - 1] == value && Map[p.X, p.Y + 1] == value) ||
                (p.Y < Rows - 2 && Map[p.X, p.Y + 1] == value && Map[p.X, p.Y + 2] == value))
                return true;

            return false;
        }

        private bool AreAdjacent(Point a, Point b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        private void SwapCells(Point a, Point b)
        {
            int temp = Map[a.X, a.Y];
            Map[a.X, a.Y] = Map[b.X, b.Y];
            Map[b.X, b.Y] = temp;
        }

        private Point WorldPosToGridPos(Vector2 worldPos)
        {
            int gridX = (int)((worldPos.X - WorldPos.X) / CellSize);
            int gridY = (int)((worldPos.Y - WorldPos.Y) / CellSize);
            return new Point(gridX, gridY);
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        private void InitializeGrid()
        {
            // Fill with 6 food types
            for (int i = 0; i < Math.Min(8, Cols); i++)
            {
                Map[i, 0] = i;
            }

            // Fill random
            for (int x = 0; x < Cols; x++)
            {
                for (int y = x < 6 ? 1 : 0; y < Rows; y++)
                {
                    Map[x, y] = GetRandomFoodType(x, y);
                }
            }
        }

        private int GetRandomFoodType(int x, int y)
        {
            int foodType = _random.Next(0, FoodTypeCount);

            while (CreatesMatch(x, y, foodType))
            {
                foodType = (foodType + 1) % FoodTypeCount;
            }

            return foodType;
        }

        private bool CreatesMatch(int x, int y, int foodType)
        {
            // Check horizontal
            if (x >= 2 && Map[x - 1, y] == foodType && Map[x - 2, y] == foodType)
                return true;

            // Check vertical
            if (y >= 2 && Map[x, y - 1] == foodType && Map[x, y - 2] == foodType)
                return true;

            return false;
        }

        private void DrawSprites(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int foodType = Map[x, y];
                    int spriteX = foodType % 3 * SpriteSize;
                    int spriteY = foodType / 3 * SpriteSize;
                    Rectangle sourceRect = new Rectangle(spriteX, spriteY, SpriteSize, SpriteSize);

                    // Center sprite in cell with padding
                    float scale = 0.95f; 
                    float spriteDrawSize = CellSize * scale;
                    float padding = (CellSize - spriteDrawSize) / 2;
                    Vector2 position = WorldPos + new Vector2(x * CellSize + padding, y * CellSize + padding);

                    spriteBatch.Draw(_spriteSheet, position, sourceRect, Color.White, 0f, Vector2.Zero,
                        spriteDrawSize / SpriteSize, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawGridLines(SpriteBatch spriteBatch)
        {
            Color lineColor = new Color(255, 255, 255, 100);

            // Vertical lines
            for (int x = 0; x <= Cols; x++)
            {
                Rectangle lineRect = new Rectangle((int)WorldPos.X + x * CellSize, (int)WorldPos.Y, 1, GridHeight);
                spriteBatch.Draw(_pixelTexture, lineRect, lineColor);
            }

            // Horizontal lines
            for (int y = 0; y <= Rows; y++)
            {
                Rectangle lineRect = new Rectangle((int)WorldPos.X, (int)WorldPos.Y + y * CellSize, GridWidth, 1);
                spriteBatch.Draw(_pixelTexture, lineRect, lineColor);
            }
        }

        private void DrawSelectedHighlight(SpriteBatch spriteBatch)
        {
            Color highlightColor = Color.Yellow;
            int thickness = 3;
            Point cell = SelectedCell.Value;
            int x = (int)WorldPos.X + cell.X * CellSize;
            int y = (int)WorldPos.Y + cell.Y * CellSize;

            // Draw border
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, CellSize, thickness), highlightColor);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + CellSize - thickness, CellSize, thickness), highlightColor);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, thickness, CellSize), highlightColor);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x + CellSize - thickness, y, thickness, CellSize), highlightColor);
        }

        private void DrawDebugMap(SpriteBatch spriteBatch)
        {
            Vector2 debugPos = new Vector2(WorldPos.X + GridWidth + 20, WorldPos.Y);
            spriteBatch.DrawString(_debugFont, "2D Map:", debugPos, Color.White);
            debugPos.Y += 20;

            for (int y = 0; y < Rows; y++)
            {
                string line = "";
                for (int x = 0; x < Cols; x++)
                {
                    line += Map[x, y] + " ";
                }
                spriteBatch.DrawString(_debugFont, line, debugPos, Color.White);
                debugPos.Y += 16;
            }
        }
    }
}
