using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BejeweledFood.Shared
{
    /// <summary>
    /// Caches loaded textures to prevent duplicate loading
    /// </summary>
    public class SpriteLoader
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, Texture2D> _cache;

        public SpriteLoader(ContentManager content)
        {
            _content = content;
            _cache = new Dictionary<string, Texture2D>();
        }

        /// <summary>
        /// Loads a texture from cache or content pipeline
        /// </summary>
        public Texture2D Load(string assetName)
        {
            if (_cache.TryGetValue(assetName, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture = _content.Load<Texture2D>(assetName);
            _cache[assetName] = texture;
            return texture;
        }

        /// <summary>
        /// Clears all cached textures
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}
