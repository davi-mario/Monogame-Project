using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaviFinalGame
{
    // Enum representing the different states of the game
    public enum GameState
    {
        MainMenu,        // The main menu of the game
        Playing,         // The player is actively playing the game
        Paused,          // The game is paused
        Options,         // The options/settings menu
        About,           // The "About" screen providing game information
        Exiting,         // The state for exiting the game
        Saving,          // The state for saving the game
        ConfirmSaving,   // State to confirm saving the game
        Loading          // The loading screen state
    }
}
