using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager : ISaveSystem
{
    private readonly string _savePath;
    private readonly IEventPublisher _eventPublisher;
    private readonly VaultSurvivors.Core.Time.ITimeManager _timeManager;

    private GameData _cachedData;

    public SaveManager(IEventPublisher eventPublisher, VaultSurvivors.Core.Time.ITimeManager timeManager)
    {
        _eventPublisher = eventPublisher;
        _timeManager = timeManager;
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void Save(GameData explicitData = null)
    {
        GameData data = explicitData != null ? explicitData : (HasSaveFile() ? Load() : new GameData());

        if (_timeManager != null)
        {
            data.lastSeenTimestamp = _timeManager.GetCurrentUnixTimestamp();
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        
        _cachedData = data; // Update cache
        Debug.Log("Game Saved to: " + _savePath);
    }

    public GameData Load()
    {
        if (_cachedData != null) return _cachedData;

        if (!HasSaveFile())
        {
            Debug.Log("[SaveManager] No save file found, returning new GameData");
            _cachedData = new GameData();
            return _cachedData;
        }

        string json = File.ReadAllText(_savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        _cachedData = data; // Cache it before firing the event to prevent loops
        _eventPublisher.Publish(new SaveDataLoadedEvent { Data = data });

        return data;
    }

    public bool HasSaveFile()
    {
        return File.Exists(_savePath);
    }

    public static long DateTimeToLong(System.DateTime time) => time.ToBinary();
    public static System.DateTime LongToDateTime(long time) => System.DateTime.FromBinary(time);

    public static float GetRemainingSeconds(long endTimeLong)
    {
        float remaining = (float)(System.DateTime.FromBinary(endTimeLong)
                                    - System.DateTime.Now).TotalSeconds;
        return Mathf.Max(0f, remaining);
    }
}
