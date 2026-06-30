using UnityEngine;
using TMPro;

public class FactoryUIDisplay : MonoBehaviour
{
    [Header("Target Tracking References")]
    [SerializeField] private StructureBase targetStructure;
    [SerializeField] private TextMeshProUGUI resourceCountText;
    [SerializeField] private TextMeshProUGUI statusSubfillText;

    private FactoryBehavior factoryBehavior;

    private void Start()
    {
        if (targetStructure != null)
        {
            factoryBehavior = targetStructure.GetBehaviour() as FactoryBehavior;
        }
    }

    private void Update()
    {
        if (factoryBehavior == null || resourceCountText == null) return;

        resourceCountText.text = $"Accumulated: {Mathf.FloorToInt(factoryBehavior.state.currentAccumulated)} / {factoryBehavior.maxStorageCapacity}";

        if (statusSubfillText != null)
        {
            statusSubfillText.text = $"Producing: {factoryBehavior.ProductionRatePerSecond} {factoryBehavior.state.resourceType}/sec (LVL {factoryBehavior.state.currentLevel})";
        }
    }
}