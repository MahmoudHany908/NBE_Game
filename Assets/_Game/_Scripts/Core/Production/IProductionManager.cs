using VaultSurvivors.Core.Production;

namespace VaultSurvivors.Core.Production
{
    public interface IProductionManager
    {
        float CalculateYield(float currentAccumulated, float ratePerSecond, float maxCapacity, float deltaTime);
        int GetUpgradeCost();
        float GetProductionRate(ResourceType type);
        float GetMaxCapacity();
        void TryUpgrade();
        void SetActiveResource(ResourceType newType);
        void CollectResources();
        void BroadcastState();
    }
}
