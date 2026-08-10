using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public PlayerData player = new PlayerData();
    public ResourceData resources = new ResourceData();
    public StructureData structures = new StructureData();
    public ProductionData production = new ProductionData();
    public WorkshopData workshop = new WorkshopData();
    public AudioData audio = new AudioData();
    public GraphicsData graphics = new GraphicsData();
    
    public long lastSeenTimestamp = 0;
}

[System.Serializable]
public class PlayerData
{
    public int level = 1;
    public int xp = 0;
    public bool hasCompletedTutorial = false;
    public bool Passed_Tutorial = false;
}

[System.Serializable]
public class ResourceData
{
    public float gold = 0f;
    public float food = 0f;
    public float energy = 0f;
    public float electronics = 0f;
    
    // enum resources
    // hashtable values
}

[System.Serializable]
public class StructureData
{
    public List<Structure> structures = new List<Structure>();
}

[System.Serializable]
public class Structure
{
    public int structureId = 0;
    public int level = 0;
    public long upgradeTimer_EndTime = 0;
}


[System.Serializable]
public class ProductionData
{
    public int globalFactoryLevel = 1;
    public string activeResourceType = "Gold";
    public float accumulatedAmount = 0f;
    public long lastCollectionTimeUnix = 0;
}

[System.Serializable]
public class WorkshopData
{
    public List<string> unlockedCharacterIds = new List<string>();
    public List<string> equippedCharacterIds = new List<string>();
}

// add market later in its sprint 

[System.Serializable]
public class AudioData
{
    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float musicVolume = 1f;
    public float ambientVolume = 1f;
    public bool isMuted = false;
}

[System.Serializable]
public class GraphicsData
{
    // 0 = Low, 1 = High
    public int qualityLevel = 1;
    public bool isHDREnabled = false;
}
