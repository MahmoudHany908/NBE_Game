using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace VaultSurvivors.Core.Production
{
    public class ProductionManager : IProductionManager, ITickable, IInitializable, IDisposable
    {
        private readonly ISaveSystem _saveSystem;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventSubscriber _eventSubscriber;

        private float _baseFoodRate = 3.0f;
        private float _baseEnergyRate = 2.5f;
        private float _baseElectronicsRate = 1.5f;
        private float _baseMaxCapacity = 100f;
        private int _baseUpgradeCost = 100;
        private float _costMultiplierPerLevel = 1.8f;
        private const int MaxLevel = 10;
        private const int VisualUpgradeLevel = 5;

        private int _globalLevel = 1;
        private ResourceType _activeResource = ResourceType.Energy;
        private float _currentAccumulated = 0f;

        [Inject]
        public ProductionManager(ISaveSystem saveSystem, IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
        {
            _saveSystem = saveSystem;
            _eventPublisher = eventPublisher;
            _eventSubscriber = eventSubscriber;
        }

        public void Initialize()
        {
            LoadState();

            _eventSubscriber.Subscribe<Command_SetProductionResource>(OnSetProductionResource);
            _eventSubscriber.Subscribe<Command_UpgradeFactory>(OnUpgradeFactory);
            _eventSubscriber.Subscribe<Command_CollectFactory>(OnCollectFactory);
            _eventSubscriber.Subscribe<Command_RequestFactoryState>(OnRequestState);
            _eventSubscriber.Subscribe<VaultSurvivors.Core.Time.AppReturnedOnlineEvent>(OnAppReturnedOnline);

            BroadcastState();
        }

        public void Dispose()
        {
            _eventSubscriber.Unsubscribe<Command_SetProductionResource>(OnSetProductionResource);
            _eventSubscriber.Unsubscribe<Command_UpgradeFactory>(OnUpgradeFactory);
            _eventSubscriber.Unsubscribe<Command_CollectFactory>(OnCollectFactory);
            _eventSubscriber.Unsubscribe<Command_RequestFactoryState>(OnRequestState);
            _eventSubscriber.Unsubscribe<VaultSurvivors.Core.Time.AppReturnedOnlineEvent>(OnAppReturnedOnline);
        }

        private void LoadState()
        {
            if (_saveSystem.HasSaveFile())
            {
                GameData gameData = _saveSystem.Load();
                _globalLevel = gameData.production.globalFactoryLevel > 0 ? gameData.production.globalFactoryLevel : 1;

                if (Enum.TryParse(gameData.production.activeResourceType, out ResourceType res))
                {
                    _activeResource = res;
                    if (_activeResource == ResourceType.Gold) _activeResource = ResourceType.Energy;
                }

                _currentAccumulated = gameData.production.accumulatedAmount;

                long currentUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long elapsedSeconds = currentUnixTime - gameData.production.lastCollectionTimeUnix;

                if (elapsedSeconds > 0 && gameData.production.lastCollectionTimeUnix > 0)
                {
                    _currentAccumulated = CalculateYield(_currentAccumulated, GetProductionRate(_activeResource), GetMaxCapacity(), elapsedSeconds);
                }
            }
        }

        private void SaveState()
        {
            GameData gameData = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
            gameData.production.globalFactoryLevel = _globalLevel;
            gameData.production.activeResourceType = _activeResource.ToString();
            gameData.production.accumulatedAmount = _currentAccumulated;
            gameData.production.lastCollectionTimeUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _saveSystem.Save(gameData);
        }

        public void Tick()
        {
            float newAccumulated = CalculateYield(_currentAccumulated, GetProductionRate(_activeResource), GetMaxCapacity(), UnityEngine.Time.deltaTime);
            if (Mathf.Abs(newAccumulated - _currentAccumulated) > 0.001f)
            {
                _currentAccumulated = newAccumulated;
                BroadcastState();
            }
        }

        public float CalculateYield(float currentAccumulated, float ratePerSecond, float maxCapacity, float deltaTime)
        {
            if (currentAccumulated >= maxCapacity) return maxCapacity;
            float newTotal = currentAccumulated + (ratePerSecond * deltaTime);
            return Mathf.Clamp(newTotal, 0f, maxCapacity);
        }

        public int GetUpgradeCost()
        {
            // Round to nearest 10 for clean UI display
            int raw = Mathf.RoundToInt(_baseUpgradeCost * Mathf.Pow(_costMultiplierPerLevel, _globalLevel - 1));
            return Mathf.Max(10, (raw / 10) * 10);
        }

        public float GetProductionRate(ResourceType type)
        {
            float baseRate = type switch
            {
                ResourceType.Food => _baseFoodRate,
                ResourceType.Energy => _baseEnergyRate,
                ResourceType.Electronics => _baseElectronicsRate,
                _ => 0f
            };
            return baseRate * Mathf.Pow(1.15f, _globalLevel - 1);
        }

        public float GetMaxCapacity()
        {
            // Round to nearest 10 for clean UI display
            int raw = Mathf.RoundToInt(_baseMaxCapacity * Mathf.Pow(1.3f, _globalLevel - 1));
            return Mathf.Max(10, (raw / 10) * 10);
        }

        private void OnSetProductionResource(Command_SetProductionResource cmd)
        {
            SetActiveResource(cmd.NewResourceType);
        }

        private void OnUpgradeFactory(Command_UpgradeFactory cmd)
        {
            TryUpgrade();
        }

        private void OnCollectFactory(Command_CollectFactory cmd)
        {
            CollectResources();
        }

        private void OnRequestState(Command_RequestFactoryState cmd)
        {
            BroadcastState();
        }

        private void OnAppReturnedOnline(VaultSurvivors.Core.Time.AppReturnedOnlineEvent evt)
        {
            if (evt.SecondsElapsedOffline > 0)
            {
                float newAccumulated = CalculateYield(_currentAccumulated, GetProductionRate(_activeResource), GetMaxCapacity(), evt.SecondsElapsedOffline);

                if (newAccumulated > _currentAccumulated)
                {
                    _currentAccumulated = newAccumulated;
                    UnityEngine.Debug.Log($"[ProductionManager] Offline progression granted for {evt.SecondsElapsedOffline} seconds.");
                    BroadcastState();
                }
            }
        }

        public void SetActiveResource(ResourceType newType)
        {
            if (newType == ResourceType.Gold) return;

            if (_activeResource != newType)
            {
                _activeResource = newType;
                SaveState();
                BroadcastState();
            }
        }

        public void TryUpgrade()
        {
            if (_globalLevel >= MaxLevel)
            {
                Debug.Log("[ProductionManager] Factory is already at max level.");
                return;
            }

            int cost = GetUpgradeCost();
            GameData gameData = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();

            if (gameData.resources.gold >= cost)
            {
                gameData.resources.gold -= cost;
                _saveSystem.Save(gameData);

                _globalLevel++;

                // Fire visual upgrade event when hitting the visual upgrade threshold
                if (_globalLevel == VisualUpgradeLevel)
                {
                    _eventPublisher.Publish(new Event_FactoryVisualUpgrade(_globalLevel));
                }

                SaveState();
                BroadcastState();
            }
            else
            {
                Debug.LogWarning($"[Vault Survivors] Not enough Gold to upgrade! Need {cost}, but have {gameData.resources.gold}.");
            }
        }

        public void CollectResources()
        {
            int amountToCollect = Mathf.FloorToInt(_currentAccumulated);
            if (amountToCollect > 0)
            {
                _currentAccumulated -= amountToCollect;
                GameData gameData = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();

                switch (_activeResource)
                {
                    case ResourceType.Food: gameData.resources.food += amountToCollect; break;
                    case ResourceType.Energy: gameData.resources.energy += amountToCollect; break;
                    case ResourceType.Electronics: gameData.resources.electronics += amountToCollect; break;
                }

                _saveSystem.Save(gameData);
                SaveState();
                BroadcastState();
            }
        }

        public void BroadcastState()
        {
            GameData gameData = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
            float gold = gameData.resources.gold;
            float food = gameData.resources.food;
            float energy = gameData.resources.energy;
            float electronics = gameData.resources.electronics;

            _eventPublisher.Publish(new Event_FactoryStateUpdated(
                _globalLevel,
                _activeResource,
                _currentAccumulated,
                GetMaxCapacity(),
                GetProductionRate(_activeResource),
                GetUpgradeCost(),
                _globalLevel >= MaxLevel,
                gold,
                food,
                energy,
                electronics
            ));
        }
    }
}