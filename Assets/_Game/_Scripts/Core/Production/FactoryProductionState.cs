using System;

namespace VaultSurvivors.Core.Production
{
    [Serializable]
    public class FactoryProductionState
    {
        public string factoryId;           
        public ResourceType resourceType;  
        public float currentAccumulated;   

        public int currentLevel;
        public long lastCollectionTimeUnix;

        public FactoryProductionState(string id, ResourceType type)
        {
            factoryId = id;
            resourceType = type;
            currentAccumulated = 0f;
            currentLevel = 1;
            lastCollectionTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}