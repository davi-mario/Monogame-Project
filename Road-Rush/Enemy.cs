using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DaviFinalGame
{
    public class Enemy
    {
        public Texture2D Texture { get; set; } // Enemy texture (sprite or animation sheet)
        public Vector2 Position { get; set; } // Enemy position on the screen
        public float Speed { get; set; } // Movement speed of the enemy
        public Vector2 Scale { get; set; } // Scale factor for the enemy sprite
        public Vector2 Direction { get; set; } = new Vector2(-1, 0); // Default direction: leftward

        private int frameWidth; // Width of each animation frame
        private int frameHeight; // Height of each animation frame
        private int currentFrame; // Current frame in the animation sequence
        private int totalFrames; // Total number of animation frames
        private float frameTimer; // Timer for controlling frame transitions
        private float frameDuration; // Duration of each frame in seconds

        // Bounding box for collision detection
        public Rectangle BoundingBox => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)(frameWidth * Scale.X),
            (int)(frameHeight * Scale.Y)
        );

        // Constructor for animated enemies
        public Enemy(Vector2 position, Texture2D texture, Vector2 direction, float speed, 
         Vector2 scale, int frameWidth, int frameHeight, float frameDuration)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture), "Texture cannot be null");
            Position = position;
            Direction = direction;
            Speed = speed;
            Scale = scale;

            // Initialize animation properties
            this.frameWidth = frameWidth > 0 ? frameWidth : texture.Width;
            this.frameHeight = frameHeight > 0 ? frameHeight : texture.Height;
            this.frameDuration = frameDuration;
            totalFrames = texture.Width / this.frameWidth; // Calculate total frames based on texture width
            currentFrame = 0; // Start with the first frame
        }

        // Constructor for non-animated enemies
        public Enemy(Texture2D texture)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture), "Texture cannot be null");
            Position = Vector2.Zero;
            Direction = new Vector2(-1, 0); // Default movement direction to left
            Speed = 100f; // Default speed
            Scale = Vector2.One; // Default scale

            // Set default frame properties for static textures
            frameWidth = texture.Width;
            frameHeight = texture.Height;
            frameDuration = 0f; // No animation
            totalFrames = 1; // Single frame
            currentFrame = 0;
        }

        // Update enemy position and animation
        public void Update(float deltaTime)
        {
            // Move the enemy in the specified direction
            Position += Direction * Speed * deltaTime;

            // Wrap the enemy to the opposite side if it moves off the screen
            if (Direction.X > 0 && Position.X > 1180) // Moving right and off screen
            {
                Position = new Vector2(-frameWidth * Scale.X, Position.Y); // Reposition to the left
            }
            else if (Direction.X < 0 && Position.X < -frameWidth * Scale.X) // Moving left and off screen
            {
                Position = new Vector2(1180, Position.Y); // Reposition to the right
            }

            // Update animation frame if animation is enabled
            if (frameDuration > 0)
            {
                frameTimer += deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0f; // Reset frame timer
                    currentFrame = (currentFrame + 1) % totalFrames; // Move to the next frame
                }
            }
        }

        // Draw the enemy on the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            // Calculate the source rectangle for the current animation frame
            Rectangle sourceRectangle = new Rectangle(
                currentFrame * frameWidth, // Frame X position in the sprite sheet
                0,
                frameWidth,
                frameHeight
            );

            // Draw the enemy with the current frame
            spriteBatch.Draw(
                Texture,
                Position,
                sourceRectangle,
                Color.White, // No tint
                0f, // No rotation
                Vector2.Zero, // Origin at the top-left corner
                Scale, // Scale the enemy sprite
                SpriteEffects.None,
                0f // Default layer depth
            );
        }

        // Check for intersection with another bounding box (collision detection)
        public bool Intersects(Rectangle other)
        {
            return BoundingBox.Intersects(other);
        }
    }
}
