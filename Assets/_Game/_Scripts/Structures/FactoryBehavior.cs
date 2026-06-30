using UnityEngine;
using VaultSurvivors.Core.Production;
using ResourceType = VaultSurvivors.Core.Production.ResourceType;

public class FactoryBehavior : MonoBehaviour, IStructureBehaviour
{
    [Header("Factory Configuration")]
    public string factoryId = "Main_Energy_Factory_01";
    public ResourceType typeToProduce = ResourceType.Energy;
    public float productionRatePerSecond = 2.5f;
    public float maxStorageCapacity = 100f;

    [Header("Upgrade Settings")]
    public ResourceType upgradeCostType = ResourceType.Gold; 
    public float baseUpgradeCost = 100f;
    public float costMultiplierPerLevel = 1.5f; 

    [Header("Live State Reference")]
    public FactoryProductionState state;

    private void Start()
    {
        state = new FactoryProductionState(factoryId, typeToProduce);
        LoadStateFromSave();
    }

    public void Tick(float deltaTime)
    {
        if (ProductionManager.Instance != null && state != null)
        {
            state.currentAccumulated = ProductionManager.Instance.CalculateYield(
                state.currentAccumulated,
                productionRatePerSecond,
                maxStorageCapacity,
                deltaTime
            );
        }
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void CollectResources()
    {
        int amountToCollect = Mathf.FloorToInt(state.currentAccumulated);
        if (amountToCollect > 0)
        {
            Debug.Log($"[Vault Survivors] Collected {amountToCollect} {state.resourceType}!");

            state.currentAccumulated -= amountToCollect;
            state.lastCollectionTimeUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (SaveManager.Instance != null)
            {
                GameData cachedData = SaveManager.Instance.HasSaveFile() ? SaveManager.Instance.Load() : new GameData();

                switch (state.resourceType)
                {
                    case ResourceType.Gold: cachedData.resources.gold += amountToCollect; break;
                    case ResourceType.Food: cachedData.resources.food += amountToCollect; break;
                    case ResourceType.Energy: cachedData.resources.energy += amountToCollect; break;
                    case ResourceType.Electronics: cachedData.resources.electronics += amountToCollect; break;
                }

                SaveManager.Instance.Save(cachedData); 
            }
        }
    }

    private void LoadStateFromSave()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasSaveFile()) return;

        GameData gameData = SaveManager.Instance.Load();
        FactoryEntry savedEntry = gameData.production.factories.Find(f => f.structureId == factoryId.GetHashCode());

        if (savedEntry != null)
        {
            state.currentAccumulated = savedEntry.accumulatedAmount;

            long currentUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = currentUnixTime - state.lastCollectionTimeUnix;

            if (elapsedSeconds > 0)
            {
                state.currentAccumulated = ProductionManager.Instance.CalculateYield(
                    state.currentAccumulated,
                    productionRatePerSecond,
                    maxStorageCapacity,
                    elapsedSeconds
                );
            }
        }
    }


    public int GetUpgradeCost()
    {
        return Mathf.FloorToInt(baseUpgradeCost * Mathf.Pow(costMultiplierPerLevel, state.currentLevel - 1));
    }

    public void TryUpgrade()
    {
        if (SaveManager.Instance == null) return;

        int cost = GetUpgradeCost();
        GameData cachedData = SaveManager.Instance.HasSaveFile() ? SaveManager.Instance.Load() : new GameData();

        float availableResources = 0f;
        switch (upgradeCostType)
        {
            case ResourceType.Gold: availableResources = cachedData.resources.gold; break;
            case ResourceType.Food: availableResources = cachedData.resources.food; break;
            case ResourceType.Energy: availableResources = cachedData.resources.energy; break;
            case ResourceType.Electronics: availableResources = cachedData.resources.electronics; break;
        }

        if (availableResources >= cost)
        {
            switch (upgradeCostType)
            {
                case ResourceType.Gold: cachedData.resources.gold -= cost; break;
                case ResourceType.Food: cachedData.resources.food -= cost; break;
                case ResourceType.Energy: cachedData.resources.energy -= cost; break;
                case ResourceType.Electronics: cachedData.resources.electronics -= cost; break;
            }
            SaveManager.Instance.Save(cachedData);

            Upgrade();
        }
        else
        {
            Debug.LogWarning($"[Vault Survivors] Not enough {upgradeCostType} to upgrade! Need {cost}, but have {availableResources}.");
        }
    }

    public void Upgrade()
    {
        state.currentLevel++;
        productionRatePerSecond *= 1.5f;
        maxStorageCapacity *= 1.25f;
        Debug.Log($"[Factory upgraded!] Level: {state.currentLevel}, Speed: {productionRatePerSecond}/s");
    }
}