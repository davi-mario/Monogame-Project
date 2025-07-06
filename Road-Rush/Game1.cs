using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // Game1.cs
    // Main game loop and core logic for Road Rush (MonoGame project)
    // -----------------------------------------------------------------------------
    // Handles initialization, content loading, update and draw cycles, and manages
    // game state transitions, player, enemies, menus, and overall gameplay flow.
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics; // Configures game graphics
        private SpriteBatch spriteBatch; // Draws 2D textures and sprites

        // Managers for game logic and UI
        private GameManager gameManager;
        private MainMenuManager mainMenuManager;
        private SaveManager saveManager;

        // Game entities
        private Player player;
        private EnemyManager enemyManager;
        private ExplosionManager explosionManager;

        // Visual assets
        private Texture2D roadTexture; // Current road texture
        private Texture2D aboutImage; // About screen image
        private Texture2D retroTexture; // Retro road style
        private Texture2D modernTexture; // Modern road style

        // Gameplay state
        private bool squirrelVisible = true; // Tracks if the player character is visible
        private bool isColliding = false; // Tracks collision state
        private int currentLevel = 1; // Current level of the game
        private bool gameWin = false; // Tracks if the player has won
        private bool isLoadingLevel = false; // Indicates if a level is loading

        // Fonts and audio
        private SpriteFont mainFont;
        private SpriteFont _menuFont;
        private Song mainMenuMusic;
        private Song transitionMusic;

        // Input and menu tracking
        private KeyboardState previousKeyboardState;
        private int loadSelectedOption = 0; // Menu selection in Load menu
        private int optionsSelectedOption = 0; // Menu selection in Options menu
        private string userInputName = string.Empty; // Player input for saving
        private string currentDateTime = string.Empty; // For timestamping saves

        // Level progression
        private int pointsToNextLevel = 1; // Points required to level up
        private double loadingTimer = 0; // Timer for loading screens
        private string loadingMessage = ""; // Message shown during loading


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true; // Enables mouse visibility in the game window

            graphics.PreferredBackBufferWidth = 1920; // Sets the preferred window width (aumentado)
            graphics.PreferredBackBufferHeight = 1080; // Sets the preferred window height (aumentado)
            graphics.ApplyChanges(); // Applies the graphics settings

            squirrelVisible = true; // Indicates whether the squirrel (player) is visible
            isColliding = false; // Indicates whether the player is currently colliding with an object
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize managers
            gameManager = new GameManager(Content);
            mainMenuManager = new MainMenuManager(Content);
            saveManager = new SaveManager();

            // Initialize and configure player
            player = new Player();
            player.LoadContent(Content);
            player.SetScreenBounds(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Initialize and load enemy manager
            enemyManager = new EnemyManager(graphics);
            enemyManager.LoadContent(Content);

            // Initialize and load explosion manager
            explosionManager = new ExplosionManager();
            explosionManager.LoadContent(Content);

            // Load textures
            roadTexture = Content.Load<Texture2D>(AssetNames.Road);
            retroTexture = Content.Load<Texture2D>(AssetNames.Road); // For Retro mode
            modernTexture = Content.Load<Texture2D>(AssetNames.ModernRoad); // For Modern mode
            aboutImage = Content.Load<Texture2D>(AssetNames.AboutImage); // About screen image

            // Load fonts
            mainFont = Content.Load<SpriteFont>(AssetNames.MainFont);
            _menuFont = Content.Load<SpriteFont>(AssetNames.MenuFont);

            // Load audio
            mainMenuMusic = Content.Load<Song>(AssetNames.MainMenuMusic);
            transitionMusic = Content.Load<Song>(AssetNames.TransitionMusic);

            // Configure music playback
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(mainMenuMusic);

            InitializeCarPositions(); // Prepare initial car positions
        }
        // Initialize car positions by setting their starting positions using the EnemyManager
        private void InitializeCarPositions()
        {
            // Retrieve the initial positions for enemies from the manager
            var initialPositions = enemyManager.GetCarPositions();

            // Set the initial positions for all enemies
            enemyManager.SetCarPositions(initialPositions);
        }

        // Update game logic every frame, handling input, game states, and progression
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle level loading state
            if (isLoadingLevel)
            {
                // Decrease loading timer
                loadingTimer -= deltaTime;

                if (loadingTimer <= 0)
                {
                    // End loading and transition to the next level
                    isLoadingLevel = false;
                    StartNextLevel();
                }
                return; // Skip the rest of the update during loading
            }

            // Handle game win state
            if (gameWin)
            {
                // Allow exiting to the Main Menu after winning
                if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                {
                    Console.WriteLine("Game Won! Returning to Main Menu.");
                    gameWin = false; // Reset the win state
                    MediaPlayer.Stop();
                    MediaPlayer.Play(mainMenuMusic); // Play main menu music
                    gameManager.SwitchState(GameState.MainMenu);
                    ResetMenuState(); // Reset menu-related state
                }
                return; // Skip further updates if the game is won
            }

            // Handle ESC key input for exiting or returning to Main Menu
            if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                switch (gameManager.CurrentState)
                {
                    case GameState.MainMenu:
                        Exit(); // Exit the game from the Main Menu
                        break;

                    case GameState.Playing:
                    case GameState.Paused:
                        // Return to the Main Menu from gameplay or pause
                        gameManager.SwitchState(GameState.MainMenu);
                        ResetMenuState();
                        break;

                    default:
                        // Default behavior is to return to the Main Menu
                        gameManager.SwitchState(GameState.MainMenu);
                        ResetMenuState();
                        break;
                }
            }

            // Handle updates based on the current game state
            if (!gameWin)
            {
                switch (gameManager.CurrentState)
                {
                    case GameState.MainMenu:
                        // Process user input in the Main Menu
                        HandleMainMenu(keyboardState);
                        break;

                    case GameState.Options:
                        // Manage input and settings in the Options menu
                        HandleOptionsState(keyboardState);
                        break;

                    case GameState.Playing:
                        // Update gameplay mechanics and check level progression
                        HandlePlayingState(keyboardState, deltaTime);
                        CheckLevelProgress(); // Check if the player should advance to the next level
                        break;

                    case GameState.Paused:
                        // Handle input during the Paused state
                        HandlePausedState(keyboardState);
                        break;

                    case GameState.Saving:
                        // Manage game saving input and logic
                        HandleSavingState(keyboardState);
                        break;

                    case GameState.Loading:
                        // Manage loading screen and input
                        HandleLoadingState(keyboardState);
                        break;

                    case GameState.About:
                        // Process input in the About screen
                        HandleAboutState(keyboardState);
                        break;
                }
            }

            // Update the previous keyboard state for the next frame
            previousKeyboardState = keyboardState;

            // Call the base class update method
            base.Update(gameTime);
        }

        // Prepares for the next level by updating textures, enemies, and difficulty.
        private void StartNextLevel()
        {
            // Update the road texture for the current level
            ChangeRoadTexture();

            // Handle level-specific enemy updates
            if (currentLevel == 2)
            {
                // Update enemies for level 2
                enemyManager.UpdateCarDirectionSpeedLevel2();
                enemyManager.AddEnemyForLevel(2, Content);
            }
            else if (currentLevel == 3)
            {
                // Add new enemies for level 3
                enemyManager.AddEnemyForLevel(3, Content);
            }

            // Increase game difficulty as levels progress
            IncreaseDifficulty();
        }

        // Checks if the player can progress to the next level based on their score.
        private void CheckLevelProgress()
        {
            // Verify if the player's score meets the threshold for the next level
            if (player.Score >= pointsToNextLevel * currentLevel)
            {
                currentLevel++; // Advance to the next level

                // Check if the player has completed the game
                if (currentLevel > 3)
                {
                    gameWin = true; // Mark the game as won
                }
                else
                {
                    // Initiate level loading state
                    isLoadingLevel = true;
                    loadingTimer = 2; // Set a 2-second loading delay
                    loadingMessage = $"Loading Level {currentLevel}...";
                    return; // Pause further logic until the loading completes
                }

                // Update road texture for the new level
                ChangeRoadTexture();

                // Configure enemies for the current level
                switch (currentLevel)
                {
                    case 2:
                        // Adjust level 2 enemies
                        enemyManager.UpdateCarDirectionSpeedLevel2();
                        enemyManager.AddEnemyForLevel(2, Content);
                        break;

                    case 3:
                        // Remove level 2 enemies and add level 3 enemies
                        enemyManager.ClearEnemiesForLevel("new.car.level2");
                        enemyManager.AddEnemyForLevel(3, Content);
                        break;
                }

                // Increase game difficulty for the new level
                IncreaseDifficulty();
            }
        }


        // Updates the road texture based on the current level
        private void ChangeRoadTexture()
        {
            switch (currentLevel)
            {
                case 2:
                    // Load the texture for level 2
                    roadTexture = Content.Load<Texture2D>("Images/road.level-3");
                    break;

                case 3:
                    // Load the texture for level 3
                    roadTexture = Content.Load<Texture2D>("Images/road.level-2");
                    break;
            }
        }

        // Adjusts game difficulty as the player progresses to higher levels
        private void IncreaseDifficulty()
        {
            switch (currentLevel)
            {
                case 2:
                    // Increase player speed and enemy speed for level 2
                    player.SetSpeed(220f);
                    enemyManager.IncreaseSpeed(50f);
                    break;

                case 3:
                    // Set maximum player speed and make enemies even faster for level 3
                    player.SetSpeed(270f);
                    enemyManager.IncreaseSpeed(100f);
                    break;
            }
        }

        // Handles input and actions for the Main Menu
        private void HandleMainMenu(KeyboardState keyboardState)
        {
            // Check if the player selects an option with Enter
            if (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                switch (mainMenuManager.SelectedOption)
                {
                    case 0: // Start the game
                        MediaPlayer.Stop();
                        MediaPlayer.Play(transitionMusic);
                        ResetGameState(); // Reset the game state
                        gameManager.SwitchState(GameState.Playing);
                        break;

                    case 1: // Load a saved game
                        saveManager.LoadSavedGamesList();
                        gameManager.SwitchState(GameState.Loading);
                        break;

                    case 2: // Open the Options menu
                        gameManager.SwitchState(GameState.Options);
                        break;

                    case 3: // How to Play
                        mainMenuManager.SelectOption(); // Ativa a tela How to Play
                        break;

                    case 4: // About
                        mainMenuManager.SelectOption(); // Ativa a tela About
                        break;

                    case 5: // Exit
                        Exit();
                        break;
                }
            }

            // Navigate up in the menu
            if (keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up))
            {
                mainMenuManager.NavigateMenu(Keys.Up);
            }

            // Navigate down in the menu
            if (keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down))
            {
                mainMenuManager.NavigateMenu(Keys.Down);
            }
        }

        // Handles input and logic for the Options menu
        private void HandleOptionsState(KeyboardState keyboardState)
        {
            // Navigate up through options
            if (keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up))
            {
                // Toggle between Retro and Modern options
                optionsSelectedOption = (optionsSelectedOption - 1 + 2) % 2;
            }

            // Navigate down through options
            if (keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down))
            {
                // Toggle between Retro and Modern options
                optionsSelectedOption = (optionsSelectedOption + 1) % 2;
            }

            // Confirm the selected option with Enter
            if (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                // Update the road texture based on the selected option
                roadTexture = optionsSelectedOption == 0 ? retroTexture : modernTexture;
                gameManager.SwitchState(GameState.MainMenu); // Return to the Main Menu
            }

            // Return to the Main Menu with Escape
            if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                gameManager.SwitchState(GameState.MainMenu);
            }
        }

        // Handles input and logic for the Paused state
        private void HandlePausedState(KeyboardState keyboardState)
        {
            // Save the game when 'S' is pressed
            if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                userInputName = string.Empty; // Reset user input for save name
                currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"); // Record the save timestamp
                gameManager.SwitchState(GameState.Saving); // Switch to the Saving state
            }

            // Resume the game when 'C' is pressed
            if (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
            {
                gameManager.SwitchState(GameState.Playing); // Return to the Playing state
            }

            // Return to the Main Menu when 'M' is pressed
            if (keyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))
            {
                MediaPlayer.Play(mainMenuMusic); // Restart the main menu music
                gameManager.SwitchState(GameState.MainMenu); // Switch to the Main Menu state
            }
        }

        // Handles input and updates during the Playing state
        private void HandlePlayingState(KeyboardState keyboardState, float deltaTime)
        {
            // Pause the game when 'P' is pressed
            if (keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                gameManager.SwitchState(GameState.Paused);
                return; // Exit early as the game is now paused
            }

            // Update the player and game logic if no explosion is active and the squirrel is visible
            if (!explosionManager.IsExploding && squirrelVisible)
            {
                // Update the player's position and check for progress
                player.Update(keyboardState, deltaTime);

                if (player.Position.Y < 0) // Player reaches the top of the screen
                {
                    player.Score++; // Increase score
                    player.PlayScoreSound(); // Play scoring sound
                    player.ResetPosition(); // Reset player position
                }

                // Update enemy positions and check for collisions
                enemyManager.Update(deltaTime);

                foreach (var enemy in enemyManager.Enemies)
                {
                    if (enemy.Intersects(player.BoundingBox))
                    {
                        // Trigger explosion and mark the squirrel as invisible
                        squirrelVisible = false;
                        explosionManager.TriggerExplosion(player.Position);
                        isColliding = true;
                        return; // Exit early as a collision occurred
                    }
                }
            }
            // Reset collision state after explosion ends
            else if (!explosionManager.IsExploding && isColliding)
            {
                player.ResetPosition(); // Reset player to start position
                squirrelVisible = true; // Make the player visible again
                isColliding = false; // Reset collision flag
            }

            // Update the explosion manager
            explosionManager.Update(deltaTime);
        }

        // Handles input and logic for the Saving state
        private void HandleSavingState(KeyboardState keyboardState)
        {
            // Iterate through all possible keys
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key))
                {
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        // Append the pressed letter to the save name
                        userInputName += key.ToString();
                    }
                    else if (key == Keys.Back && userInputName.Length > 0)
                    {
                        // Remove the last character when Backspace is pressed
                        userInputName = userInputName.Remove(userInputName.Length - 1);
                    }
                    else if (key == Keys.Enter && !string.IsNullOrWhiteSpace(userInputName))
                    {
                        // Determine the current road type for saving
                        string roadType = roadTexture == retroTexture ? "Retro" : // Menu Option
                                          roadTexture == modernTexture ? "Modern" : // Menu Option
                                          roadTexture == Content.Load<Texture2D>("Images/road.level-2") ? "Level2" :
                                          roadTexture == Content.Load<Texture2D>("Images/road.level-3") ? "Level3" :
                                          "Level1";

                        // Save the game using SaveManager
                        saveManager.SaveGame(userInputName, player.Score, currentLevel, roadType);

                        // Return to the Paused state after saving
                        gameManager.SwitchState(GameState.Paused);

                        // Clear the input name for future saves
                        userInputName = string.Empty;
                    }
                    else if (key == Keys.Escape)
                    {
                        // Cancel saving and return to the Paused state
                        gameManager.SwitchState(GameState.Paused);
                        userInputName = string.Empty;
                    }
                }
            }
        }

        // Handles input for the About state
        private void HandleAboutState(KeyboardState keyboardState)
        {
            // Return to the Main Menu when Escape is pressed
            if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                gameManager.SwitchState(GameState.MainMenu);
            }
        }

        // Handles input and logic for the Loading state
        private void HandleLoadingState(KeyboardState keyboardState)
        {
            // Retrieve the saved games ranking list
            var ranking = saveManager.LoadRanking();

            // Navigate up through the save list
            if (keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up))
            {
                loadSelectedOption = (loadSelectedOption - 1 + ranking.Count) % ranking.Count;
            }
            // Navigate down through the save list
            else if (keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down))
            {
                loadSelectedOption = (loadSelectedOption + 1) % ranking.Count;
            }
            // Load the selected save when Enter is pressed
            else if (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                // Locate the selected save from the ranking
                var selectedGame = ranking[loadSelectedOption];
                int saveIndex = saveManager.SavedGames.FindIndex(s => s.Contains(selectedGame.Name));

                if (saveIndex >= 0)
                {
                    var loadedGame = saveManager.LoadGame(saveIndex);
                    if (loadedGame.HasValue)
                    {
                        // Update the game state with loaded data
                        player.Score = loadedGame.Value.Score;
                        currentLevel = loadedGame.Value.Level;

                        // Set the road texture based on the saved road type
                        roadTexture = loadedGame.Value.RoadType switch
                        {
                            "Retro" => retroTexture,
                            "Modern" => modernTexture,
                            "Level2" => Content.Load<Texture2D>("Images/road.level-2"),
                            "Level3" => Content.Load<Texture2D>("Images/road.level-3"),
                            _ => Content.Load<Texture2D>("Images/Road") // Default texture
                        };

                        // Play the transition music
                        MediaPlayer.Stop();
                        MediaPlayer.Play(transitionMusic);
                        MediaPlayer.IsRepeating = true;

                        Console.WriteLine($"Game loaded: Name: {loadedGame.Value.Name}, Score: {loadedGame.Value.Score}, " +
                        $"Level: {loadedGame.Value.Level}, RoadType: {loadedGame.Value.RoadType}");
                        gameManager.SwitchState(GameState.Playing);
                    }
                    else
                    {
                        Console.WriteLine("Error loading the game. The save data is incomplete or corrupted.");
                    }
                }
            }
            // Delete the selected save when Delete is pressed
            else if (keyboardState.IsKeyDown(Keys.Delete) && previousKeyboardState.IsKeyUp(Keys.Delete))
            {
                saveManager.DeleteGame(loadSelectedOption);
                saveManager.LoadSavedGamesList();
                loadSelectedOption = Math.Min(loadSelectedOption, ranking.Count - 1);
            }
            // Return to the Main Menu when Escape is pressed
            else if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                gameManager.SwitchState(GameState.MainMenu);
            }
        }
        // Resets the state of the main menu
        private void ResetMenuState()
        {
            Console.WriteLine("Resetting menu state.");
            previousKeyboardState = Keyboard.GetState(); // Updates the keyboard state
            mainMenuManager.Reset(); // Resets the main menu state
        }

        // Resets the game to its initial state
        private void ResetGameState()
        {
            currentLevel = 1; // Resets to level 1
            player.Score = 0; // Resets the player's score to 0
            squirrelVisible = true; // Makes the player visible
            isColliding = false; // Clears any collision state
            gameWin = false; // Removes the win state

            // Reset the road texture based on the selected option or default
            roadTexture = optionsSelectedOption == 0 ? retroTexture : modernTexture;

            // Clears all enemies and recreates them for level 1
            enemyManager.ClearEnemies(); // Removes all enemies
            enemyManager.AddEnemyForLevel(1, Content); // Adds enemies specific to level 1

            // Reset flags for level-specific enemies
            enemyManager.ResetLevelSpecificFlags(); // Resets `isLevel2EnemyAdded` and `isLevel3EnemyAdded`

            InitializeCarPositions(); // Repositions the cars to their starting positions

            Console.WriteLine("Game state reset: Level 1, score reset to 0.");
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); // Clears the screen with a black background

            spriteBatch.Begin();

            // Display a loading message if the next level is loading
            if (isLoadingLevel)
            {
                Vector2 messageSize = _menuFont.MeasureString(loadingMessage);

                Vector2 position = new Vector2(
                    (graphics.PreferredBackBufferWidth - messageSize.X) / 2,
                    (graphics.PreferredBackBufferHeight - messageSize.Y) / 2
                );

                spriteBatch.DrawString(_menuFont, loadingMessage, position, Color.Yellow);
                spriteBatch.End(); // Ends the drawing for loading
                return; // Stops further drawing while loading
            }

            // Display a victory message if the game is won
            if (gameWin)
            {
                GraphicsDevice.Clear(Color.Black); // Black background for the victory screen

                Vector2 winMessageSize = _menuFont.MeasureString("YOU WIN!");
                Vector2 exitMessageSize = mainFont.MeasureString("Press ESC to Exit");

                Vector2 winMessagePosition = new Vector2(
                    (graphics.PreferredBackBufferWidth - winMessageSize.X) / 3 + 15 ,
                    graphics.PreferredBackBufferHeight / 4
                );

                Vector2 exitMessagePosition = new Vector2(
                    (graphics.PreferredBackBufferWidth - exitMessageSize.X) / 3,
                    graphics.PreferredBackBufferHeight / 3 + 50
                );

                spriteBatch.DrawString(_menuFont, "YOU WIN! Score: 3", winMessagePosition, Color.Yellow);
                spriteBatch.DrawString(mainFont, "     Press ESC to Exit", exitMessagePosition, Color.White);
                spriteBatch.End();
                return; // Prevents further game drawing after a win
            }

            // Handle the game states
            if (gameManager.CurrentState == GameState.Saving)
            {
                // Draw saving interface
                spriteBatch.DrawString(mainFont, "Saving Game:", new Vector2(50, 50), Color.Yellow);
                spriteBatch.DrawString(mainFont, $"Name: {userInputName}", new Vector2(50, 100), Color.White);
                spriteBatch.DrawString(mainFont, "Type a name and press Enter to save, or Esc to cancel.",
                    new Vector2(50, graphics.PreferredBackBufferHeight - 50), Color.Yellow);
            }
            else if (gameManager.CurrentState == GameState.Loading)
            {
                // Draw the saved games and rankings
                spriteBatch.DrawString(mainFont, "Position ranking and saved games - selected to continue:", new Vector2(50, 50), Color.Yellow);

                var ranking = saveManager.LoadRanking(); // Retrieve the ranking, sorted by high score

                for (int i = 0; i < ranking.Count; i++)
                {
                    var entry = ranking[i];
                    var color = (i == loadSelectedOption) ? Color.Green : Color.White;

                    // Display saved game name, score, level, and timestamp
                    spriteBatch.DrawString(mainFont,
                        $"{i + 1}: {entry.Name} - {entry.Score} Points - Level {entry.Level} - {entry.SaveTime}",
                        new Vector2(70, 100 + i * 30), color);
                }

                spriteBatch.DrawString(mainFont, "Press Enter to Load, Delete to Remove, or Esc to Return",
                    new Vector2(50, graphics.PreferredBackBufferHeight - 50), Color.Yellow);
            }
            else if (gameManager.CurrentState == GameState.MainMenu)
            {
                // Draw the main menu
                mainMenuManager.Draw(spriteBatch, graphics);
            }
            else if (gameManager.CurrentState == GameState.Options)
            {
                // Draw the options menu
                spriteBatch.DrawString(mainFont, "OPTIONS:", new Vector2(100, 100), Color.Yellow);
                spriteBatch.DrawString(mainFont, $"{(optionsSelectedOption == 0 ? "> " : "  ")}Retro", new Vector2(100, 200), Color.White);
                spriteBatch.DrawString(mainFont, $"{(optionsSelectedOption == 1 ? "> " : "  ")}Modern", new Vector2(100, 250), Color.White);
                spriteBatch.DrawString(mainFont, "Press Enter to Select\nPress Esc to Return", new Vector2(100, 400), Color.Green);
            }
            else if (gameManager.CurrentState == GameState.About)
            {
                // Draw the About screen with scaled content
                int scaledWidth = (int)(graphics.PreferredBackBufferWidth * 0.85);
                int scaledHeight = (int)(graphics.PreferredBackBufferHeight * 0.85);
                int posX = (graphics.PreferredBackBufferWidth - scaledWidth) / 2;
                int posY = (graphics.PreferredBackBufferHeight - scaledHeight) / 2;

                Rectangle scaledRectangle = new Rectangle(posX, posY, scaledWidth, scaledHeight);
                spriteBatch.Draw(aboutImage, scaledRectangle, Color.White);
                spriteBatch.DrawString(mainFont, "Press Esc to Return", new Vector2(585, graphics.PreferredBackBufferHeight - 40), Color.Yellow);
            }
            else if (gameManager.CurrentState == GameState.Playing || gameManager.CurrentState == GameState.Paused)
            {
                // Draw the road and game objects
                spriteBatch.Draw(roadTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                enemyManager.Draw(spriteBatch);

                if (squirrelVisible)
                    player.Draw(spriteBatch);

                explosionManager.Draw(spriteBatch);

                // Draw score and level on a dark background
                Texture2D backgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
                backgroundTexture.SetData(new[] { Color.Black });

                Vector2 scoreTextSize = mainFont.MeasureString($"Score: {player.Score}");
                Vector2 levelTextSize = mainFont.MeasureString($"Level: {currentLevel}");

                spriteBatch.Draw(backgroundTexture,
                    new Rectangle(10, 10, (int)scoreTextSize.X + 10, (int)scoreTextSize.Y + 5),
                    Color.DarkGray);
                spriteBatch.DrawString(mainFont, $"Score: {player.Score}", new Vector2(15, 12), Color.White);

                spriteBatch.Draw(backgroundTexture,
                    new Rectangle(1300, 10, (int)levelTextSize.X + 10, (int)levelTextSize.Y + 5),
                    Color.DarkGray);
                spriteBatch.DrawString(mainFont, $"Level: {currentLevel}", new Vector2(1305, 12), Color.White);

                if (gameManager.CurrentState == GameState.Paused)
                {
                    // Display paused menu
                    string pausedText = "PAUSED\nPress C to Continue\nPress M to Main Menu\nPress S to Save Game";

                    // Measure text size for centering
                    Vector2 textSize = mainFont.MeasureString(pausedText);
                    Vector2 textPosition = new Vector2(
                        (graphics.PreferredBackBufferWidth - textSize.X) / 2,
                        (graphics.PreferredBackBufferHeight - textSize.Y) / 2
                    );

                    // Draw a black background behind the text
                    Texture2D pauseBackgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
                    pauseBackgroundTexture.SetData(new[] { Color.Black });

                    spriteBatch.Draw(
                        pauseBackgroundTexture,
                        new Rectangle((int)textPosition.X - 10, (int)textPosition.Y - 10, (int)textSize.X + 20, (int)textSize.Y + 20),
                        Color.Black
                    );

                    // Draw the paused text
                    spriteBatch.DrawString(mainFont, pausedText, textPosition, Color.White);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
