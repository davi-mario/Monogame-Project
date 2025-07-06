using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

// -----------------------------------------------------------------------------
// Player.cs
// Represents the player character, handles movement, animation, and scoring.
// -----------------------------------------------------------------------------
// Author: Davi Henrique
// -----------------------------------------------------------------------------

    // Player class represents the main character controlled by the user
    public class Player
    {
        public Vector2 Position { get; set; } // Player's position
        public int Score { get; set; } // Player's score
        private float _speed; // Movement speed
        private Vector2 _scale; // Sprite scale
        private Texture2D _spriteSheetRight, _spriteSheetLeft, _currentSpriteSheet; // Sprite sheets
        private SoundEffect _scoreSound; // Sound effect for scoring
        private int _frameWidth, _frameHeight, _currentFrame, _totalFrames; // Animation properties
        private float _frameTime, _timer; // Animation timing
        private int _screenWidth, _screenHeight; // Screen bounds
        private bool _canScore; // Flag to allow scoring

        // Constructor to initialize the player with default attributes and animation settings
        public Player()
        {
            _speed = 200f; // Set the default movement speed
            _scale = new Vector2(1.8f, 1.8f); // Define the scaling for the player's sprite
            Score = 0; // Initialize the player's score to zero
            _canScore = true; // Allow scoring at the start of the game

            _frameWidth = 40; // Width of animation frame
            _frameHeight = 64; // Height of animation frame
            _currentFrame = 0; // Start the animation at the first frame
            _totalFrames = 3; // Total number of frames in the animation cycle
            _frameTime = 0.1f; // Time interval between frame transitions
            _timer = 0f; // Initialize the animation timer
        }


        // Set the player's speed
        public void SetSpeed(float newSpeed)
        {
            _speed = newSpeed;
        }

        // Load textures and sound effects for the player
        public void LoadContent(ContentManager content)
        {
            _spriteSheetRight = content.Load<Texture2D>(AssetNames.PlayerRight);
            _spriteSheetLeft = content.Load<Texture2D>(AssetNames.PlayerLeft);
            _currentSpriteSheet = _spriteSheetRight;
            _scoreSound = content.Load<SoundEffect>(AssetNames.ScoreSound);
        }

        // Set screen boundaries for movement and reset position
        public void SetScreenBounds(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            ResetPosition();
        }

        // Update player movement and animation
        public void Update(KeyboardState keyboardState, float deltaTime)
        {
            Vector2 newPosition = Position;
            bool isMoving = false;

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)) newPosition.Y -= _speed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                newPosition.X -= _speed * deltaTime;
                isMoving = true;
                _currentSpriteSheet = _spriteSheetLeft;
            }
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                newPosition.X += _speed * deltaTime;
                isMoving = true;
                _currentSpriteSheet = _spriteSheetRight;
            }
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down)) newPosition.Y += _speed * deltaTime;

            // Update animation if moving
            if (isMoving)
            {
                _timer += deltaTime;
                if (_timer >= _frameTime)
                {
                    _currentFrame = (_currentFrame + 1) % _totalFrames;
                    _timer = 0f;
                }
            }
            else
            {
                _currentFrame = 0;
            }

            // Clamp position and handle scoring
            newPosition.X = MathHelper.Clamp(newPosition.X, 0, _screenWidth - _frameWidth * _scale.X);
            if (newPosition.Y < -_frameHeight && _canScore)
            {
                Score++;
                PlayScoreSound();
                _canScore = false;
                ResetPosition();
                return;
            }
            if (newPosition.Y >= _screenHeight - _frameHeight * _scale.Y - 20) _canScore = true;
            newPosition.Y = MathHelper.Clamp(newPosition.Y, float.MinValue, _screenHeight - _frameHeight * _scale.Y - 10);

            Position = newPosition;
        }

        // Reset position to the start point
        public void ResetPosition()
        {
            Position = new Vector2((_screenWidth - _frameWidth * _scale.X) / 2, _screenHeight - _frameHeight * _scale.Y - 10);
        }

        // Play the scoring sound effect
        public void PlayScoreSound()
        {
            _scoreSound?.Play();
        }

        // Get the player's bounding box for collision detection
        public Rectangle BoundingBox =>
            new Rectangle((int)Position.X, (int)Position.Y, (int)(_frameWidth * _scale.X), (int)(_frameHeight * _scale.Y));

        // Draw the player on the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_currentSpriteSheet, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        }
    }

