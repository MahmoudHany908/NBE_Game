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
}
#endif
