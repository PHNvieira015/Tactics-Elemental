using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Application.persistentDataPath + "/save.json";

    public static void Save(SavedData data)
    {
        try
        {
            // Convert data to JSON
            string json = JsonUtility.ToJson(data, true);

            // Write JSON to file
            File.WriteAllText(SavePath, json);

            Debug.Log("Game saved to: " + SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game data: " + e.Message);
        }
    }

    public static SavedData Load()
    {
        try
        {
            // Check if the save file exists
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found. Creating new game data.");
                return new SavedData(); // Use the parameterless constructor
            }

            // Read JSON from file
            string json = File.ReadAllText(SavePath);

            // Convert JSON to SavedData
            SavedData data = JsonUtility.FromJson<SavedData>(json);

            Debug.Log("Game loaded from: " + SavePath);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load game data: " + e.Message);
            return new SavedData(); // Use the parameterless constructor
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete save file: " + e.Message);
        }
    }
}