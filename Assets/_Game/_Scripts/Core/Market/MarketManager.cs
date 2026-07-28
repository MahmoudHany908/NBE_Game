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

            RandomizePrices(); // initial randomization
        }

        public void Dispose()
        {
            _eventSubscriber.Unsubscribe<Command_SellResource>(OnSellResource);
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

            BroadcastPrices();
        }

        private float GetRandomizedPrice(float basePrice)
        {
            // +/- 20%
            float modifier = UnityEngine.Random.Range(0.8f, 1.2f);
            return basePrice * modifier;
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
                _saveSystem.Save(gameData);
                
                // Fire an event if other systems need to know immediately, 
                // but the UI typically polls or listens to a global event. 
                // We'll leave it as saved for now.
                Debug.Log($"[MarketManager] Sold {cmd.Amount} {cmd.Resource} for {goldGained} Gold.");
                
                // Let's broadcast the factory state update so Production UI updates if open
                // In a full implementation, there might be a separate wallet update event.
                _eventPublisher.Publish(new Command_RequestFactoryState()); 
            }
            else
            {
                Debug.LogWarning($"[MarketManager] Not enough {cmd.Resource} to sell {cmd.Amount}.");
            }
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
