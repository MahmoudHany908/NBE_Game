using System.Collections.Generic;
using NUnit.Framework;

[System.Serializable]
public class GameData
{
    public PlayerData player = new PlayerData();
    public ProductionData production = new ProductionData();
    public Market market = new Market();
    public ResourceData resources = new ResourceData();
    public TimerData timers = new TimerData();
}

[System.Serializable]
public class PlayerData
{
    public int level = 1;
    public int xp = 0;
}

[System.Serializable]
public class ProductionData
{
    public List<Factory> factories = new List<Factory>();
    // collect all structures here and name it StructureData 
}

[System.Serializable]
public class Factory
{
    public int factoryId = 0;
    public int level = 0;
    public string selectedResource = "";
    public float Amount = 0f;
    public bool isProducing = false;
}

[System.Serializable]
public class Market
{
    public int MarketId = 0;
    public int level = 0;
    public string selectedResource = "";
    public bool isSelling = false;
}

[System.Serializable]
public class ResearchHub
{
    public int hubId = 0;
    public int level = 0;
    public long endTime = 0;
}

[System.Serializable]
public class ResourceData
{
    public float GoldAmount = 0f;
    public float FoodAmount = 0f;
    public float EnergyAmount = 0f;
    public float ElectronicsAmount = 0f;
}

[System.Serializable]
public class TimerData
{
    public long upgradeTimerEndTime = 0;
    // wtf is this ??!!!!
}