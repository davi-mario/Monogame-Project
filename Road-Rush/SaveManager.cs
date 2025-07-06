using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DaviFinalGame
{
    // -----------------------------------------------------------------------------
    // SaveManager.cs
    // Handles saving, loading, ranking, and deleting game progress (JSON files).
    // -----------------------------------------------------------------------------
    // Author: Davi Henrique
    // -----------------------------------------------------------------------------
    public class SaveManager
    {
        private const string DefaultSaveFolder = "Saves";
        private readonly string _saveFolderPath; // Path to the folder where save files are stored
        public List<string> SavedGames { get; private set; } // List of saved game files

        // Constructor to initialize the save manager and ensure the save folder exists
        public SaveManager(string saveFolderPath = DefaultSaveFolder)
        {
            _saveFolderPath = saveFolderPath;
            SavedGames = new List<string>();

            // Create the save folder if it doesn't exist
            if (!Directory.Exists(_saveFolderPath))
                Directory.CreateDirectory(_saveFolderPath);

            LoadSavedGamesList(); // Load the list of saved games
        }

        // Load the list of saved games from the save folder
        public void LoadSavedGamesList()
        {
            SavedGames.Clear();

            if (Directory.Exists(_saveFolderPath))
            {
                // Add all .json files in the save folder to the list
                foreach (var file in Directory.GetFiles(_saveFolderPath, "*.json"))
                {
                    SavedGames.Add(Path.GetFileName(file));
                }
            }
        }

        // Save the current game state to a JSON file
        public void SaveGame(string userName, int score, int level, string roadType)
        {
            // Validate the player's name
            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Error: Invalid player name.");
                return;
            }

            // Create an object containing the save data
            var saveData = new
            {
                Name = userName, // Player's name
                Score = score,   // Player's score
                Level = level,   // Current level
                RoadType = roadType, // Current road type
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm") // Time of save
            };

            // Generate a unique save file name
            string saveFileName = Path.Combine(_saveFolderPath, $"{userName}_{DateTime.Now:yyyy-MM-dd_HH-mm}.json");

            try
            {
                // Serialize save data to JSON and write to file
                string jsonContent = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileName, jsonContent);
                Console.WriteLine($"Game successfully saved: {saveFileName}");
                LoadSavedGamesList(); // Refresh the saved games list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving the game: {ex.Message}");
            }
        }

        // Load a saved game from the specified index
        public (string Name, int Score, int Level, string SaveTime, string RoadType)? LoadGame(int saveIndex)
        {
            // Validate the save index
            if (saveIndex < 0 || saveIndex >= SavedGames.Count)
            {
                Console.WriteLine("Error: Invalid save index.");
                return null;
            }

            string filePath = Path.Combine(_saveFolderPath, SavedGames[saveIndex]);

            try
            {
                // Deserialize the save data from JSON
                var saveData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(filePath));

                // Ensure all required keys are present
                if (saveData.ContainsKey("Name") && saveData.ContainsKey("Score") &&
                    saveData.ContainsKey("Level") && saveData.ContainsKey("SaveTime") &&
                    saveData.ContainsKey("RoadType"))
                {
                    // Extract and return save data
                    string name = saveData["Name"].GetString();
                    int score = saveData["Score"].GetInt32();
                    int level = saveData["Level"].GetInt32();
                    string saveTime = saveData["SaveTime"].GetString();
                    string roadType = saveData["RoadType"].GetString();

                    return (name, score, level, saveTime, roadType);
                }
                else
                {
                    Console.WriteLine("Error: Save file is corrupted or incomplete.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading save file '{filePath}': {ex.Message}");
                return null;
            }
        }

        // Load the ranking of saved games, ordered by score
        public List<(string Name, int Score, int Level, string SaveTime, string RoadType)> LoadRanking()
        {
            var ranking = new List<(string Name, int Score, int Level, string SaveTime, string RoadType)>();

            foreach (var file in Directory.GetFiles(_saveFolderPath, "*.json"))
            {
                try
                {
                    // Deserialize each save file and add it to the ranking
                    var saveData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(file));

                    if (saveData.ContainsKey("Name") && saveData.ContainsKey("Score") &&
                        saveData.ContainsKey("Level") && saveData.ContainsKey("SaveTime") &&
                        saveData.ContainsKey("RoadType"))
                    {
                        string name = saveData["Name"].GetString();
                        int score = saveData["Score"].GetInt32();
                        int level = saveData["Level"].GetInt32();
                        string saveTime = saveData["SaveTime"].GetString();
                        string roadType = saveData["RoadType"].GetString();

                        ranking.Add((name, score, level, saveTime, roadType));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading save file '{file}': {ex.Message}");
                }
            }

            // Sort the ranking by score in descending order
            return ranking.OrderByDescending(r => r.Score).ToList();
        }

        // Delete a saved game by its index in the list
        public void DeleteGame(int saveIndex)
        {
            // Validate the save index
            if (saveIndex < 0 || saveIndex >= SavedGames.Count)
            {
                Console.WriteLine("Error: Invalid save index.");
                return;
            }

            string filePath = Path.Combine(_saveFolderPath, SavedGames[saveIndex]);

            try
            {
                // Delete the file if it exists
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Successfully deleted save file '{SavedGames[saveIndex]}'.");
                    LoadSavedGamesList(); // Refresh the saved games list
                }
                else
                {
                    Console.WriteLine($"Error: File '{filePath}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting save file: {ex.Message}");
            }
        }
    }
}
