using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // MainMenuManager.cs
    // Handles main menu navigation, rendering, and About screen logic.
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------
    // Main menu manager class to handle menu navigation and rendering
    public class MainMenuManager
    {
        private SpriteFont _menuFont; // Font for menu options
        private SpriteFont _headerFont; // Font for menu titles or headers
        private Texture2D _logoTexture; // Texture for the logo
        private Texture2D _aboutImage; // Image for the About screen
        private string[] _menuOptions = { "Play", "Load", "Options", "How to Play", "About", "Exit" }; // Menu options
        private int _selectedOption = 0; // Current selected menu option
        private bool _isAboutActive = false; // State to track if About screen is active
        private bool _isHowToPlayActive = false; // State to track if How to Play screen is active

        // Constructor to load fonts and textures
        public MainMenuManager(ContentManager content)
        {
            _menuFont = content.Load<SpriteFont>(AssetNames.MenuFont); // Load the menu font
            _headerFont = content.Load<SpriteFont>(AssetNames.MainFont); // Load the header font
            _logoTexture = content.Load<Texture2D>(AssetNames.Logo); // Load the logo texture
            _aboutImage = content.Load<Texture2D>(AssetNames.AboutImage); // Load the About image
        }

        // Property to get the currently selected option
        public int SelectedOption => _selectedOption;

        // Property to check if About screen is active
        public bool IsAboutActive => _isAboutActive;

        // Reset menu state to defaults
        public void Reset()
        {
            _selectedOption = 0; // Reset to the first menu option
            _isAboutActive = false; // Deactivate About screen
            _isHowToPlayActive = false; // Deactivate How to Play screen
        }

        // Navigate through menu options based on key input
        public void NavigateMenu(Keys key)
        {
            if (_isAboutActive || _isHowToPlayActive)
            {
                // Exit About or How to Play screen when Escape key is pressed
                if (key == Keys.Escape)
                {
                    _isAboutActive = false;
                    _isHowToPlayActive = false;
                }
                return;
            }

            // Navigate down through menu options
            if (key == Keys.Down)
            {
                _selectedOption = (_selectedOption + 1) % _menuOptions.Length;
            }
            // Navigate up through menu options
            else if (key == Keys.Up)
            {
                _selectedOption = (_selectedOption - 1 + _menuOptions.Length) % _menuOptions.Length;
            }
        }

        // Handle selection of a menu option
        public void SelectOption()
        {
            if (_selectedOption == 3) // How to Play option
            {
                _isHowToPlayActive = true; // Activate How to Play screen
            }
            else if (_selectedOption == 4) // About option (agora é o índice 4)
            {
                _isAboutActive = true; // Activate About screen
            }
            else if (_selectedOption == 5) // Exit option (agora é o índice 5)
            {
                Environment.Exit(0); // Close the application
            }
            // Additional menu options can be implemented here
        }

        // Render the main menu or the About screen
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (_isHowToPlayActive)
            {
                DrawHowToPlay(spriteBatch, graphics); // Draw the How to Play screen
            }
            else if (_isAboutActive)
            {
                DrawAbout(spriteBatch, graphics); // Draw the About screen
            }
            else
            {
                DrawMenu(spriteBatch, graphics); // Draw the main menu
            }
        }

        // Render the main menu
        private void DrawMenu(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            float scaleX = 1.0f; // Horizontal scale for the logo
            float scaleY = 0.5f; // Vertical scale for the logo
            float logoVerticalOffset = 80; // Vertical offset for the logo

            // Calculate logo position
            var logoPosition = new Vector2(
                (graphics.PreferredBackBufferWidth - _logoTexture.Width * scaleX) / 2,
                logoVerticalOffset
            );

            // Draw the logo
            spriteBatch.Draw(
                _logoTexture,
                logoPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(scaleX, scaleY),
                SpriteEffects.None,
                0f
            );

            // Draw menu options
            float menuStartY = logoPosition.Y + (_logoTexture.Height * scaleY) + 50;
            float optionSpacing = 38f; // Espaçamento vertical ainda menor
            float normalScale = 0.5f; // Fonte ainda menor para opções não selecionadas
            float selectedScale = 0.8f; // Fonte menor para a opção selecionada

            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Color color = (i == _selectedOption) ? Color.Yellow : Color.White;
                float scale = (i == _selectedOption) ? selectedScale : normalScale;
                Vector2 size = _menuFont.MeasureString(_menuOptions[i]) * scale;
                Vector2 pos = new Vector2(
                    (graphics.PreferredBackBufferWidth - size.X) / 2,
                    menuStartY + i * optionSpacing
                );
                spriteBatch.DrawString(_menuFont, _menuOptions[i], pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        // Render the About screen
        private void DrawAbout(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Calculate position for About image
            var position = new Vector2(
                (graphics.PreferredBackBufferWidth - _aboutImage.Width) / 2,
                (graphics.PreferredBackBufferHeight - _aboutImage.Height) / 2
            );

            // Draw the About image
            spriteBatch.Draw(_aboutImage, position, Color.White);

            // Draw the "Press ESC to return" message
            string returnMessage = "Press ESC to Return";
            Vector2 messageSize = _menuFont.MeasureString(returnMessage);
            Vector2 messagePosition = new Vector2(
                (graphics.PreferredBackBufferWidth - messageSize.X) / 2,
                position.Y + _aboutImage.Height + 20
            );
            spriteBatch.DrawString(_menuFont, returnMessage, messagePosition, Color.Yellow);
        }

        // Render the How to Play screen
        private void DrawHowToPlay(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            string[] lines = new string[]
            {
                "How to Play",
                "",
                "Objective: Help the squirrel cross the road without being hit by enemy cars and obstacles.",
                "Controls: Use the arrow keys (Up, Down, Left, Right) to move the squirrel.",
                "Scoring: Each time the squirrel crosses the road, you earn 1 point. With 2 points, you advance to the next level.",
                "Game Over: You lose if an enemy (car or obstacle) touches the squirrel.",
                "",
                "Press ESC to return to the menu."
            };
            float startY = graphics.PreferredBackBufferHeight / 4f;
            for (int i = 0; i < lines.Length; i++)
            {
                Color color = (i == 0) ? Color.Yellow : Color.White;
                Vector2 size = _menuFont.MeasureString(lines[i]);
                Vector2 pos = new Vector2((graphics.PreferredBackBufferWidth - size.X) / 2, startY + i * 40);
                spriteBatch.DrawString(_menuFont, lines[i], pos, color);
            }
        }
    }
}