using UnityEngine;
using VaultSurvivors.Core.Production;

public class DummyFactoryTest : MonoBehaviour
{
    [Header("Fake Factory Data")]
    public ResourceType typeToProduce = ResourceType.Energy;
    public float productionRatePerSecond = 2.5f;
    public float maxStorageCapacity = 100f;

    [Header("Live Output")]
    public float currentlyAccumulated = 0f;

    private void Update()
    {
        // Ask the ProductionManager to do the math every frame
        if (ProductionManager.Instance != null)
        {
            currentlyAccumulated = ProductionManager.Instance.CalculateYield(
                currentlyAccumulated,
                productionRatePerSecond,
                maxStorageCapacity,
                Time.deltaTime
            );
        }
    }

    // Temporary test method to act like a player clicking "Collect"
    public void Collect()
    {
        int amountToCollect = Mathf.FloorToInt(currentlyAccumulated);
        if (amountToCollect > 0)
        {
            Debug.Log($"Collected {amountToCollect} {typeToProduce}!");
            currentlyAccumulated -= amountToCollect;
        }
    }
}