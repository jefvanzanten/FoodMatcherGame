using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BejeweledFood.Shared
{
    public enum AnimationState
    {
        Idle,
        Moving,
        Destroying
    }

    public class FoodItem
    {
        private const float MoveDuration = 0.3f; // seconds
        private const float DestroyDuration = 0.4f; // seconds
        private const int SpriteSize = 60;

        public int FoodType { get; set; }
        public Point GridPosition { get; set; }
        public Vector2 WorldPos { get; private set; }
        public AnimationState CurrentAnimation { get; private set; }

        // Animation properties
        public float Scale { get; private set; }
        public float Rotation { get; private set; }
        public float Opacity { get; private set; }

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private float _animationProgress;

        public FoodItem(int foodType, Point gridPosition, Vector2 worldPos)
        {
            FoodType = foodType;
            GridPosition = gridPosition;
            WorldPos = worldPos;
            CurrentAnimation = AnimationState.Idle;

            // Default visual state
            Scale = 1f;
            Rotation = 0f;
            Opacity = 1f;
            _animationProgress = 0f;
        }

        public void StartMove(Vector2 targetWorldPos)
        {
            _startPos = WorldPos;
            _targetPos = targetWorldPos;
            _animationProgress = 0f;
            CurrentAnimation = AnimationState.Moving;
        }

        public void StartDestroy()
        {
            _animationProgress = 0f;
            CurrentAnimation = AnimationState.Destroying;
        }

        public void Update(GameTime gameTime)
        {
            switch (CurrentAnimation)
            {
                case AnimationState.Moving:
                    UpdateMoveAnimation(gameTime);
                    break;
                case AnimationState.Destroying:
                    UpdateDestroyAnimation(gameTime);
                    break;
            }
        }

        private void UpdateMoveAnimation(GameTime gameTime)
        {
            _animationProgress += (float)gameTime.ElapsedGameTime.TotalSeconds / MoveDuration;

            if (_animationProgress >= 1f)
            {
                _animationProgress = 1f;
                WorldPos = _targetPos;
                CurrentAnimation = AnimationState.Idle;
            }
            else
            {
                // Smooth interpolation (ease-out)
                float t = EaseOutQuad(_animationProgress);
                WorldPos = Vector2.Lerp(_startPos, _targetPos, t);
            }
        }

        private void UpdateDestroyAnimation(GameTime gameTime)
        {
            _animationProgress += (float)gameTime.ElapsedGameTime.TotalSeconds / DestroyDuration;

            if (_animationProgress >= 1f)
            {
                _animationProgress = 1f;
                Scale = 0f;
                Opacity = 0f;
                // Animation complete - Grid should handle removal
            }
            else
            {
                // Scale down and fade out
                float t = EaseInQuad(_animationProgress);
                Scale = 1f - t;
                Opacity = 1f - t;
                Rotation = t * MathHelper.TwoPi; // Spin while destroying
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D spriteSheet, int cellSize)
        {
            if (Opacity <= 0f) return; // Don't draw if fully transparent

            int spriteX = FoodType % 3 * SpriteSize;
            int spriteY = FoodType / 3 * SpriteSize;
            Rectangle sourceRect = new Rectangle(spriteX, spriteY, SpriteSize, SpriteSize);

            // Calculate draw properties
            float scaleFactor = 0.95f;
            float spriteDrawSize = cellSize * scaleFactor * Scale;
            float padding = (cellSize - (cellSize * scaleFactor)) / 2;
            Vector2 position = WorldPos + new Vector2(padding, padding);

            // Origin at center for rotation
            Vector2 origin = new Vector2(SpriteSize / 2f, SpriteSize / 2f);
            Vector2 centerPos = position + new Vector2(cellSize * scaleFactor / 2f, cellSize * scaleFactor / 2f);

            Color drawColor = Color.White * Opacity;

            spriteBatch.Draw(
                spriteSheet,
                centerPos,
                sourceRect,
                drawColor,
                Rotation,
                origin,
                spriteDrawSize / SpriteSize,
                SpriteEffects.None,
                0f
            );
        }

        // Easing functions
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseInQuad(float t) => t * t;
    }
}