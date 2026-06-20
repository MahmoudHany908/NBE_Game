using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour 
{
    public static SaveManager Instance { get; private set; }

    private string savePath;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.Log("object destroyed!!!");
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void Save(GameData data)
    {
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
        return JsonUtility.FromJson<GameData>(json);
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
    
    public long DateTimeToLong(DateTime time)
    {
        return time.ToBinary();
    }

    public DateTime LongToDateTime(long time)
    {
        return DateTime.FromBinary(time);
    }

    public float GetRemainingSeconds(long endTimeLong)
    {
        DateTime endTime = DateTime.FromBinary(endTimeLong);
        float remaining = (float)(endTime - DateTime.Now).TotalSeconds;
        return (int)Mathf.Max(0f, remaining);
    }
}
