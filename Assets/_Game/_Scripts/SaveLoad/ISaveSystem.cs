public interface ISaveSystem
{
    void Save(GameData explicitData = null);
    GameData Load();
    bool HasSaveFile();
}