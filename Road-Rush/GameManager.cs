using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // GameManager.cs
    // Manages the current game state and background music.
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------
    public class GameManager
    {
        // Represents the current state of the game (e.g., MainMenu, Playing, etc.)
        public GameState CurrentState { get; set; }

        private Song _homeScreenMusic; // Background music for the home screen

        // Constructor initializes the game state and loads home screen music
        public GameManager(ContentManager content)
        {
            CurrentState = GameState.MainMenu; // Default state is Main Menu

            // Load the home screen music
            _homeScreenMusic = content.Load<Song>(AssetNames.MainMenuMusic);

            // Configure the MediaPlayer for continuous background music
            MediaPlayer.IsRepeating = true; // Repeat the music
            MediaPlayer.Volume = 0.5f; // Set the music volume
            MediaPlayer.Play(_homeScreenMusic); // Start playing the home screen music
        }

        // Switch the game to a new state
        public void SwitchState(GameState newState)
        {
            CurrentState = newState; // Update the current game state
        }
    }
}
