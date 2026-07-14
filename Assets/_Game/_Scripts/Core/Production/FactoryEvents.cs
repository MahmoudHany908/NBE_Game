using VaultSurvivors.Core.Production;

namespace VaultSurvivors.Core.Production
{
    public class Command_SetProductionResource : IGameEvent
    {
        public ResourceType NewResourceType { get; }

        public Command_SetProductionResource(ResourceType newResourceType)
        {
            NewResourceType = newResourceType;
        }
    }

    public class Command_UpgradeFactory : IGameEvent { }

    public class Command_CollectFactory : IGameEvent { }

    public class Event_FactoryStateUpdated : IGameEvent
    {
        public int GlobalLevel { get; }
        public ResourceType ActiveResource { get; }
        public float CurrentAccumulation { get; }
        public float MaxCapacity { get; }
        public float ProductionRate { get; }
        public int UpgradeCost { get; }
        public float WalletGold { get; }
        public float WalletFood { get; }
        public float WalletEnergy { get; }
        public float WalletElectronics { get; }

        public Event_FactoryStateUpdated(int globalLevel, ResourceType activeResource, float currentAccumulation, float maxCapacity, float productionRate, int upgradeCost, float walletGold, float walletFood, float walletEnergy, float walletElectronics)
        {
            GlobalLevel = globalLevel;
            ActiveResource = activeResource;
            CurrentAccumulation = currentAccumulation;
            MaxCapacity = maxCapacity;
            ProductionRate = productionRate;
            UpgradeCost = upgradeCost;
            WalletGold = walletGold;
            WalletFood = walletFood;
            WalletEnergy = walletEnergy;
            WalletElectronics = walletElectronics;
        }
    }
}
