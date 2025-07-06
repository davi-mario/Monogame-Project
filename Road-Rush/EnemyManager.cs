using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // EnemyManager.cs
    // Manages all enemy cars: creation, update, drawing, and level-specific logic.
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------

    // Struct para dados iniciais dos inimigos
    public struct EnemyInitData
    {
        public Vector2 Position;
        public Vector2 Scale;
        public float Speed;
        public string AssetName;
    }

    // Manages the enemies in the game, including their properties, positions, and behaviors
    public class EnemyManager
    {
        // List of enemies currently in the game
        public List<Enemy> Enemies { get; private set; }

        // Graphics manager for handling screen dimensions
        private readonly GraphicsDeviceManager graphics;

        // Lista de dados iniciais dos inimigos
        private readonly List<EnemyInitData> initialEnemies;

        // Flags to track if specific level enemies have been added
        private bool isLevel2EnemyAdded = false;
        private bool isLevel3EnemyAdded = false;

        // Flag to track if reverse textures are loaded for level 2
        private bool isReverseTextureLoaded = false;

        // Constructor initializes enemy properties and default values
        public EnemyManager(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
            Enemies = new List<Enemy>();

            // Inicializa dados dos inimigos
            initialEnemies = new List<EnemyInitData>
            {
                new EnemyInitData { Position = new Vector2(1180, 100), Scale = new Vector2(0.4f, 0.5f), Speed = 350f, AssetName = AssetNames.BlueCar },
                new EnemyInitData { Position = new Vector2(1180, 230), Scale = new Vector2(0.9f, 1.05f), Speed = 280f, AssetName = AssetNames.GrayCar },
                new EnemyInitData { Position = new Vector2(1180, 360), Scale = new Vector2(0.9f, 1.1f), Speed = 350f, AssetName = AssetNames.RedCar },
                new EnemyInitData { Position = new Vector2(1180, 500), Scale = new Vector2(0.4f, 0.5f), Speed = 550f, AssetName = AssetNames.YellowCar }
            };
        }

        // Loads content and initializes enemies with their textures, positions, scales, and speeds
        public void LoadContent(ContentManager content)
        {
            // Cria inimigos a partir dos dados iniciais
            foreach (var data in initialEnemies)
            {
                var texture = content.Load<Texture2D>(data.AssetName);
                var enemy = new Enemy(texture)
                {
                    Position = data.Position,
                    Speed = data.Speed,
                    Scale = data.Scale,
                    Direction = new Vector2(-1, 0) // Moving left
                };
                Enemies.Add(enemy);
            }
        }

        // Updates the state of all enemies
        public void Update(float deltaTime)
        {
            // Update all enemies
            foreach (var enemy in Enemies)
            {
                enemy.Update(deltaTime);
            }
        }

        // Increases the speed of all enemies
        public void IncreaseSpeed(float speedIncrement)
        {
            // Increase speed for all enemies
            foreach (var enemy in Enemies)
            {
                enemy.Speed += speedIncrement;
            }
        }

        // Draws all enemies on the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw all enemies
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }
        // Updates directions and speeds for enemies specific to level 2
        public void UpdateCarDirectionSpeedLevel2()
        {
            // Ensure the required number of enemies are present
            if (Enemies.Count >= 4)
            {
                // Confirm we are modifying only the enemies with reverse textures
                if (Enemies[1].Texture.Name.EndsWith("reverse") && Enemies[3].Texture.Name.EndsWith("reverse"))
                {
                    Enemies[1].Direction *= -1; // Reverse direction
                    Enemies[1].Speed += 300f;  // Increase speed

                    Enemies[3].Direction *= -1;
                    Enemies[3].Speed += 300f;

                    Console.WriteLine("Updated directions, speeds, and textures for Level 2.");
                }
                else
                {
                    Console.WriteLine("Skipped update: Non-reverse textures detected.");
                }
            }
        }

        // Retrieves the positions of all enemies
        public List<Vector2> GetCarPositions()
        {
            // Create a list to store the positions of enemies
            var positions = new List<Vector2>();

            // Loop through each enemy and extract its position
            foreach (var enemy in Enemies)
            {
                positions.Add(enemy.Position);
            }

            // Return the list of positions
            return positions;
        }

        // Retrieves the speeds of all enemies
        public List<float> GetCarSpeeds()
        {
            // Create a list to store the speeds of enemies
            var carSpeeds = new List<float>();

            // Loop through each enemy and extract its speed
            foreach (var enemy in Enemies)
            {
                carSpeeds.Add(enemy.Speed);
            }

            // Return the list of speeds
            return carSpeeds;
        }

        // Sets new positions for all enemies
        public void SetCarPositions(List<Vector2> positions)
        {
            // Check if the input list matches the number of enemies
            if (positions == null || positions.Count != Enemies.Count)
            {
                Console.WriteLine("Error: Provided positions do not match the number of enemies.");
                return;
            }

            // Update each enemy's position using the provided list
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Position = positions[i];
                Console.WriteLine($"Car {i} position restored to X={positions[i].X}, Y={positions[i].Y}");
            }
        }

        // Sets new speeds for all enemies
        public void SetCarSpeeds(List<float> speeds)
        {
            // Check if the input list matches the number of enemies
            if (speeds == null || speeds.Count != Enemies.Count)
            {
                Console.WriteLine("Error: Provided speeds do not match the number of enemies.");
                return;
            }

            // Update each enemy's speed using the provided list
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Speed = speeds[i];
                Console.WriteLine($"Car {i} speed restored to {speeds[i]}");
            }
        }

        // Adds a new car enemy with specified properties
        public void AddCar(Vector2 position, float speed, Vector2 direction, Texture2D texture, Vector2 scale)
        {
            // Ensure the provided texture is valid
            if (texture == null)
            {
                Console.WriteLine("Error: Texture cannot be null.");
                return;
            }

            // Create a new enemy object with the provided properties
            var newCar = new Enemy(texture)
            {
                Position = position,
                Speed = speed,
                Direction = direction,
                Scale = scale
            };

            // Add the new car enemy to the list of enemies
            Enemies.Add(newCar);

            Console.WriteLine($"Added new car at Position: {position}, Speed: {speed}, Direction: {direction}.");
        }

        // Adds enemies specific to a level
        public void AddEnemyForLevel(int level, ContentManager content)
        {
            // Add enemies for level 1
            if (level == 1)
            {
                // Ensure that level 1 enemies are not added multiple times
                if (Enemies.Count > 0)
                {
                    Console.WriteLine("Enemies for level 1 are already loaded.");
                    return;
                }

                // Load textures for level 1 cars and create enemies
                var carTextures = new List<Texture2D>
        {
            content.Load<Texture2D>("Images/blue.car"),
            content.Load<Texture2D>("Images/gray.car"),
            content.Load<Texture2D>("Images/red.car"),
            content.Load<Texture2D>("Images/yellow.car")
        };

                // Initialize enemies using predefined positions, scales, and speeds
                for (int i = 0; i < carTextures.Count; i++)
                {
                    Enemies.Add(new Enemy(carTextures[i])
                    {
                        Position = initialEnemies[i].Position,
                        Scale = initialEnemies[i].Scale,
                        Speed = initialEnemies[i].Speed,
                        Direction = new Vector2(-1, 0) // Move left
                    });
                }

                Console.WriteLine("Level 1 enemies added.");
            }
            else if (level == 2)
            {
                // Prevent adding level 2 enemies multiple times
                if (isLevel2EnemyAdded)
                {
                    Console.WriteLine("Level 2 enemies already added.");
                    return;
                }

                // Load textures for level 2, ensuring reverse textures are used
                if (!isReverseTextureLoaded)
                {
                    // Remove any existing enemies with unwanted textures
                    Enemies.RemoveAll(enemy =>
                        enemy.Texture.Name.Contains("gray.car") ||
                        enemy.Texture.Name.Contains("yellow.car"));

                    // Load reverse textures for level 2
                    var grayCarReverseTexture = content.Load<Texture2D>("Images/gray.car.reverse");
                    var yellowCarReverseTexture = content.Load<Texture2D>("Images/yellow.car.reverse");

                    // Add reverse texture enemies for level 2
                    Enemies.Add(new Enemy(grayCarReverseTexture)
                    {
                        Position = new Vector2(-200, 570), // Start from the left
                        Scale = new Vector2(0.9f, 1.1f),
                        Speed = 520f,
                        Direction = new Vector2(1, 0) // Move right
                    });

                    Enemies.Add(new Enemy(yellowCarReverseTexture)
                    {
                        Position = new Vector2(-200, 500),
                        Scale = new Vector2(0.28f, 0.3f),
                        Speed = 380f,
                        Direction = new Vector2(1, 0)
                    });

                    isReverseTextureLoaded = true; // Mark reverse textures as loaded
                    Console.WriteLine("Reverse textures loaded for Level 2.");
                }

                // Add a unique enemy for level 2
                var level2Texture = content.Load<Texture2D>("Images/new.car.level2");
                Enemies.Add(new Enemy(level2Texture)
                {
                    Position = new Vector2(1180, 220),
                    Scale = new Vector2(0.23f, 0.23f),
                    Speed = 720f,
                    Direction = new Vector2(-1, 0) // Move left
                });

                isLevel2EnemyAdded = true;
                Console.WriteLine("Level 2 enemies added, including new.car.level2.");
            }
            else if (level == 3)
            {
                // Clear level 2 enemies before adding level 3
                ClearEnemiesForLevel("new.car.level2");

                // Prevent adding level 3 enemies multiple times
                if (isLevel3EnemyAdded)
                {
                    Console.WriteLine("Level 3 enemies already added.");
                    return;
                }

                // Add animated enemies for level 3
                var enemyTexture = content.Load<Texture2D>("Images/new.enemy(1)");
                int frameWidth = enemyTexture.Width / 4; // Assuming 4 frames horizontally
                int frameHeight = enemyTexture.Height;  // Full texture height

                // Add an animated enemy at the specified position with defined speed, scale, frame dimensions, and frame duration
                AddAnimatedEnemy(new Vector2(1180, 0), enemyTexture, 400f, new Vector2(0.6f, 0.6f), frameWidth, frameHeight, 0.1f);
                AddAnimatedEnemy(new Vector2(1180, 180), enemyTexture, 550f, new Vector2(1.0f, 1.1f), frameWidth, frameHeight, 0.1f);

                // Mark that level 3 enemies have been successfully added
                isLevel3EnemyAdded = true;

                // Log the addition of level 3 enemies and the removal of level 2 specific enemies
                Console.WriteLine("Level 3 enemies added and new.car.level2 removed.");
            }
        }

        // Adds an animated enemy with specified properties
        private void AddAnimatedEnemy(Vector2 position, Texture2D texture, float speed, Vector2 scale, int frameWidth, int frameHeight, float frameDuration)
        {
            // Ensure the texture is valid
            if (texture == null)
            {
                Console.WriteLine("Error: Texture cannot be null.");
                return;
            }

            // Create and add an animated enemy
            var enemy = new Enemy(position, texture, new Vector2(-1, 0), speed, scale, frameWidth, frameHeight, frameDuration);
            Enemies.Add(enemy);

            // Log information about the newly added animated enemy, including its position, speed, and scale
            Console.WriteLine($"Added animated enemy at Position: {position}, Speed: {speed}, Scale: {scale}.");

        }

        // Removes enemies based on their texture identifier
        public void ClearEnemiesForLevel(string textureIdentifier)
        {
            // Remove all enemies whose texture name contains the specified identifier
            Enemies.RemoveAll(enemy => enemy.Texture.Name.Contains(textureIdentifier));

            // Log the removal of enemies matching the given texture identifier
            Console.WriteLine($"Removed all enemies with texture containing '{textureIdentifier}'.");
        }

        // Removes all enemies from the game
        public void ClearEnemies()
        {
            Enemies.Clear();
            //Console.WriteLine("All enemies cleared.");
        }

        public void ResetLevelSpecificFlags()
        {
            isLevel2EnemyAdded = false;
            isLevel3EnemyAdded = false;
            Console.WriteLine("Level-specific flags reset.");
        }

    }
}
