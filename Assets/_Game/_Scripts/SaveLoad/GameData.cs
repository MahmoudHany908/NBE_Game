using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public PlayerData player = new PlayerData();
    public ResourceData resources = new ResourceData();
    public StructureData structures = new StructureData();
    public ProductionData production = new ProductionData();
    public AudioData audio = new AudioData();
}

[System.Serializable]
public class PlayerData
{
    public int level = 1;
    public int xp = 0;
}

[System.Serializable]
public class ResourceData
{
    public float gold = 0f;
    public float food = 0f;
    public float energy = 0f;
    public float electronics = 0f;
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
    public List<FactoryEntry> factories = new List<FactoryEntry>();
}

[System.Serializable]
public class FactoryEntry : Structure
{
    public string selectedResource = "";
    public float accumulatedAmount = 0f;
    public bool isProducing = false;
}

// add market later in its sprint 
// add researchHub later in its sprint


[System.Serializable]
public class AudioData
{
    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float musicVolume = 1f;
    public float ambientVolume = 1f;
    public bool isMuted = false;
}