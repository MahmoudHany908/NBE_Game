using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static event Action<GameData> OnSave;
    public static event Action<GameData> OnLoad;

    private string savePath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void Save()
    {
        GameData data = HasSaveFile() ? Load() : new GameData();
        OnSave?.Invoke(data);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved to: " + savePath);
    }

    public GameData Load()
    {
        if (!HasSaveFile())
        {
            Debug.Log("No save file found, returning new GameData");
            return new GameData();
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        OnLoad?.Invoke(data);
        return data;
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public static long DateTimeToToLong(DateTime time) => time.ToBinary();
    public static DateTime LongToDateTime(long time) => DateTime.FromBinary(time);

    public static float GetRemainingSeconds(long endTimeLong)
    {
        float remaining = (float)(DateTime.FromBinary(endTimeLong) - DateTime.Now).TotalSeconds;
        return Mathf.Max(0f, remaining);
    }
}
