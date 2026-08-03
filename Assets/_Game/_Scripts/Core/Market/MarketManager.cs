using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VaultSurvivors.Core.Production;

namespace VaultSurvivors.Core.Market
{
    public class MarketManager : IMarketManager, ITickable, IInitializable, IDisposable
    {
        private readonly ISaveSystem _saveSystem;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventSubscriber _eventSubscriber;

        private float _timeSinceLastUpdate = 0f;
        private readonly float _updateInterval = 15f;

        private float _baseFoodPrice = 10f;
        private float _baseEnergyPrice = 15f;
        private float _baseElectronicsPrice = 25f;

        private float _currentFoodPrice;
        private float _currentEnergyPrice;
        private float _currentElectronicsPrice;

        private float _previousFoodPrice;
        private float _previousEnergyPrice;
        private float _previousElectronicsPrice;

        [Inject]
        public MarketManager(ISaveSystem saveSystem, IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
        {
            _saveSystem = saveSystem;
            _eventPublisher = eventPublisher;
            _eventSubscriber = eventSubscriber;
        }

        public void Initialize()
        {
            // Initial prices
            _currentFoodPrice = _baseFoodPrice;
            _currentEnergyPrice = _baseEnergyPrice;
            _currentElectronicsPrice = _baseElectronicsPrice;

            _previousFoodPrice = _baseFoodPrice;
            _previousEnergyPrice = _baseEnergyPrice;
            _previousElectronicsPrice = _baseElectronicsPrice;

            _eventSubscriber.Subscribe<Command_SellResource>(OnSellResource);
            _eventSubscriber.Subscribe<Command_RequestMarketPrices>(OnRequestPrices);

            RandomizePrices(); // initial randomization
        }

        public void Dispose()
        {
            _eventSubscriber.Unsubscribe<Command_SellResource>(OnSellResource);
            _eventSubscriber.Unsubscribe<Command_RequestMarketPrices>(OnRequestPrices);
        }

        public void Tick()
        {
            _timeSinceLastUpdate += UnityEngine.Time.deltaTime;
            if (_timeSinceLastUpdate >= _updateInterval)
            {
                _timeSinceLastUpdate = 0f;
                RandomizePrices();
            }
        }

        private void RandomizePrices()
        {
            _previousFoodPrice = _currentFoodPrice;
            _previousEnergyPrice = _currentEnergyPrice;
            _previousElectronicsPrice = _currentElectronicsPrice;

            _currentFoodPrice = GetRandomizedPrice(_baseFoodPrice);
            _currentEnergyPrice = GetRandomizedPrice(_baseEnergyPrice);
            _currentElectronicsPrice = GetRandomizedPrice(_baseElectronicsPrice);

            // Guarantee at least 1 resource moves in a different direction.
            // If all 3 went up (or stayed same) relative to their previous price,
            // force one random resource to go below its previous price, and vice versa.
            bool foodUp = _currentFoodPrice >= _previousFoodPrice;
            bool energyUp = _currentEnergyPrice >= _previousEnergyPrice;
            bool electronicsUp = _currentElectronicsPrice >= _previousElectronicsPrice;

            if (foodUp == energyUp && energyUp == electronicsUp)
            {
                // All moved in the same direction — flip one at random
                int flipIndex = UnityEngine.Random.Range(0, 3);
                switch (flipIndex)
                {
                    case 0:
                        _currentFoodPrice = ForceOppositeDirection(_baseFoodPrice, _previousFoodPrice, foodUp);
                        break;
                    case 1:
                        _currentEnergyPrice = ForceOppositeDirection(_baseEnergyPrice, _previousEnergyPrice, energyUp);
                        break;
                    case 2:
                        _currentElectronicsPrice = ForceOppositeDirection(_baseElectronicsPrice, _previousElectronicsPrice, electronicsUp);
                        break;
                }
            }

            BroadcastPrices();
        }

        private float GetRandomizedPrice(float basePrice)
        {
            // ±12% swing, rounded to nearest whole integer
            float modifier = UnityEngine.Random.Range(0.88f, 1.12f);
            return Mathf.RoundToInt(basePrice * modifier);
        }

        /// <summary>
        /// Forces a price to move in the opposite direction of its current trend.
        /// If it went up, generate a price below the previous; if down, above it.
        /// </summary>
        private float ForceOppositeDirection(float basePrice, float previousPrice, bool wasUp)
        {
            float newPrice;
            if (wasUp)
            {
                // Force down: generate in the lower portion of the range
                float modifier = UnityEngine.Random.Range(0.88f, 0.99f);
                newPrice = Mathf.RoundToInt(basePrice * modifier);
                // Ensure it's actually below the previous price
                if (newPrice >= previousPrice) newPrice = Mathf.Max(Mathf.RoundToInt(basePrice * 0.88f), 1f);
            }
            else
            {
                // Force up: generate in the upper portion of the range
                float modifier = UnityEngine.Random.Range(1.01f, 1.12f);
                newPrice = Mathf.RoundToInt(basePrice * modifier);
                // Ensure it's actually above the previous price
                if (newPrice <= previousPrice) newPrice = Mathf.RoundToInt(basePrice * 1.12f);
            }
            return newPrice;
        }

        private void OnSellResource(Command_SellResource cmd)
        {
            if (cmd.Amount <= 0) return;

            GameData gameData = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();

            float goldGained = 0f;
            bool success = false;

            if (cmd.Resource == ResourceType.Food && gameData.resources.food >= cmd.Amount)
            {
                gameData.resources.food -= cmd.Amount;
                goldGained = cmd.Amount * _currentFoodPrice;
                success = true;
            }
            else if (cmd.Resource == ResourceType.Energy && gameData.resources.energy >= cmd.Amount)
            {
                gameData.resources.energy -= cmd.Amount;
                goldGained = cmd.Amount * _currentEnergyPrice;
                success = true;
            }
            else if (cmd.Resource == ResourceType.Electronics && gameData.resources.electronics >= cmd.Amount)
            {
                gameData.resources.electronics -= cmd.Amount;
                goldGained = cmd.Amount * _currentElectronicsPrice;
                success = true;
            }

            if (success)
            {
                gameData.resources.gold += goldGained;
                
                // EXPLICIT SAVE VERIFICATION
                // This ensures the transaction is written to disk immediately
                _saveSystem.Save(gameData);
                
                Debug.Log($"[MarketManager] Sold {cmd.Amount} {cmd.Resource} for {goldGained} Gold.");
                
                // Broadcast state update so Production UI updates if open
                _eventPublisher.Publish(new Command_RequestFactoryState()); 
            }
            else
            {
                Debug.LogWarning($"[MarketManager] Not enough {cmd.Resource} to sell {cmd.Amount}.");
            }
        }

        private void OnRequestPrices(Command_RequestMarketPrices cmd)
        {
            BroadcastPrices();
        }

        private void BroadcastPrices()
        {
            _eventPublisher.Publish(new Event_MarketPricesUpdated(
                _currentFoodPrice, _currentEnergyPrice, _currentElectronicsPrice,
                _previousFoodPrice, _previousEnergyPrice, _previousElectronicsPrice
            ));
        }
    }
}
