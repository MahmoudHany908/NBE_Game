using UnityEngine;
using VaultSurvivors.Core.Production;
using ResourceType = VaultSurvivors.Core.Production.ResourceType;

public class FactoryBehavior : MonoBehaviour, IStructureBehaviour
{
    [Header("Factory Configuration")]
    public string factoryId = "Main_Energy_Factory_01";
    public ResourceType typeToProduce = ResourceType.Energy;
    private float productionRatePerSecond = 2.5f;
    public float maxStorageCapacity = 100f;
    public float ProductionRatePerSecond
    {
        get => productionRatePerSecond;
        set
        {
            productionRatePerSecond = value;
            Upgrade();
        }
    }
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

                SaveManager.Instance.Save();
            }
        }
    }

    // OFFLINE PROGRESSION CALCULATOR: (RUns instantly on startup)
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
                Debug.Log($"[Offline Processing] {factoryId} generated resources offline for {elapsedSeconds} seconds!");
            }
        }
    }

    [ContextMenu("Upgrade")]
    public void Upgrade()
    {
        state.currentLevel++;
        productionRatePerSecond *= 1.5f; 
        maxStorageCapacity *= 1.25f;     
        Debug.Log($"[Factory upgraded!] Level: {state.currentLevel}, Speed: {productionRatePerSecond}/s");
    }
}