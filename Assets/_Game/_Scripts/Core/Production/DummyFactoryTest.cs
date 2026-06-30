using UnityEngine;
using VaultSurvivors.Core.Production;

public class DummyFactoryTest : MonoBehaviour
{
    [Header("Factory Configuration")]
    public string myFactoryId = "Main_Energy_Factory_01";
    public ResourceType typeToProduce = ResourceType.Energy;
    public float productionRatePerSecond = 2.5f;
    public float maxStorageCapacity = 100f;

    [Header("The Logbook (Live State)")]
    // This is the object we will save and load including all the data regarding the factory's state
    public FactoryProductionState state;

    private void Start()
    {

        state = new FactoryProductionState(myFactoryId, typeToProduce);
    }

    private void Update()
    {
        if (ProductionManager.Instance != null && state != null)
        {
            state.currentAccumulated = ProductionManager.Instance.CalculateYield(
                state.currentAccumulated,
                productionRatePerSecond,
                maxStorageCapacity,
                Time.deltaTime
            );
        }
    }

    public void Collect()
    {
        int amountToCollect = Mathf.FloorToInt(state.currentAccumulated);
        if (amountToCollect > 0)
        {
            Debug.Log($"[Vault Survivors] Collected {amountToCollect} {state.resourceType}!");
            state.currentAccumulated -= amountToCollect;
            state.lastCollectionTimeUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}