using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // ExplosionManager.cs
    // Handles explosion effects: animation, sound, and rendering.
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------
    public class ExplosionManager
    {
        private Texture2D _texture; // Texture for the explosion sprite
        private SoundEffect _sound; // Sound effect for the explosion
        private Vector2 _position; // Position of the explosion
        private float _scale; // Current scale of the explosion
        private float _maxScale; // Maximum scale of the explosion
        private float _timer; // Timer to track the duration of the explosion
        private float _duration; // Total duration of the explosion animation
        private bool _isExploding; // Indicates if an explosion is active

        // Property to check if an explosion is currently active
        public bool IsExploding => _isExploding;

        // Constructor to initialize explosion parameters
        public ExplosionManager()
        {
            _maxScale = 0.3f; // Maximum size of the explosion
            _duration = 1.0f; // Duration of the explosion animation
            _isExploding = false; // Explosion is initially inactive
        }

        // Load explosion assets (texture and sound)
        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>(AssetNames.ExplosionImage); // Load explosion texture
            _sound = content.Load<SoundEffect>(AssetNames.ExplosionSound); // Load explosion sound
        }

        // Trigger a new explosion at a specific position
        public void TriggerExplosion(Vector2 position)
        {
            _isExploding = true; // Set explosion state to active
            _position = position; // Set the position of the explosion
            _scale = 0.1f; // Start with an initial small scale
            _timer = 0; // Reset the timer
            _sound.Play(); // Play the explosion sound effect
        }

        // Update the explosion animation
        public void Update(float deltaTime)
        {
            if (_isExploding)
            {
                _timer += deltaTime; // Increment the timer
                _scale += deltaTime * (_maxScale / _duration); // Gradually increase the scale

                // Check if the explosion duration has elapsed
                if (_timer >= _duration)
                {
                    _isExploding = false; // End the explosion
                }
            }
        }

        // Draw the explosion sprite
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isExploding)
            {
                spriteBatch.Draw(
                    _texture, // Explosion texture
                    _position, // Explosion position
                    null, // Use the entire texture
                    Color.White, // No tint
                    0f, // No rotation
                    new Vector2(_texture.Width / 2, _texture.Height / 2), // Center of the texture as the origin
                    new Vector2(_scale, _scale), // Scale the explosion dynamically
                    SpriteEffects.None,
                    0f // Default layer depth
                );
            }
        }
    }
}
