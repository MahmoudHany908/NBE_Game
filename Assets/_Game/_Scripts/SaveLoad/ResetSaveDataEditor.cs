#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ResetSaveDataEditor
{
    [MenuItem("Tools/Reset Save Data")]
    public static void ResetSaveData()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Deleted save file at: {path}");
            EditorUtility.DisplayDialog("Save Data Reset", "Your save data has been successfully deleted. You will start fresh on the next run.", "OK");
        }
        else
        {
            Debug.LogWarning($"[SaveManager] No save file found at: {path}");
            EditorUtility.DisplayDialog("Save Data Reset", "No save file exists. You are already starting fresh.", "OK");
        }
    }

    [MenuItem("Tools/Add 50,000 Gold")]
    public static void AddGold()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            if (data != null)
            {
                data.resources.gold += 50000;
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
                Debug.Log("[SaveManager] Added 50,000 Gold to save file.");
                EditorUtility.DisplayDialog("Cheat Added", "50,000 Gold has been added to your save file!", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "No save file exists! Press Play and enter the game once to create a save file before adding gold.", "OK");
        }
        
    }
}
#endif
