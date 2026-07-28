using VaultSurvivors.Core.Production;

namespace VaultSurvivors.Core.Market
{
    public class Command_SellResource : IGameEvent
    {
        public ResourceType Resource { get; }
        public int Amount { get; }

        public Command_SellResource(ResourceType resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }
    }

    public class Event_MarketPricesUpdated : IGameEvent
    {
        public float FoodPrice { get; }
        public float EnergyPrice { get; }
        public float ElectronicsPrice { get; }
        
        public float PreviousFoodPrice { get; }
        public float PreviousEnergyPrice { get; }
        public float PreviousElectronicsPrice { get; }

        public Event_MarketPricesUpdated(float foodPrice, float energyPrice, float electronicsPrice, 
                                         float prevFood, float prevEnergy, float prevElectronics)
        {
            FoodPrice = foodPrice;
            EnergyPrice = energyPrice;
            ElectronicsPrice = electronicsPrice;
            PreviousFoodPrice = prevFood;
            PreviousEnergyPrice = prevEnergy;
            PreviousElectronicsPrice = prevElectronics;
        }
    }

    public class Command_RequestMarketPrices : IGameEvent { }
    public class Command_OpenMarketUI : IGameEvent { }
    public class Command_CloseMarketUI : IGameEvent { }
}
