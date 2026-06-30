using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VaultSurvivors.Core.Production;
using ResourceType = VaultSurvivors.Core.Production.ResourceType; // Prevents the Mono.Cecil conflict!

[RequireComponent(typeof(PanelUI))]
public class FactoryUIDisplay : MonoBehaviour
{
    [Header("Target Tracking References")]
    [SerializeField] private StructureBase targetStructure;
    [SerializeField] private TextMeshProUGUI resourceCountText;
    [SerializeField] private TextMeshProUGUI statusSubfillText;

    [Header("Upgrade UI")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    [Header("Collect UI")]
    [SerializeField] private Button collectButton;

    [Header("Wallet UI")]
    [SerializeField] private TextMeshProUGUI playerWalletText;

    private PanelUI _panelUI;
    private FactoryBehavior _factoryBehavior;

    private void Awake()
    {
        _panelUI = GetComponent<PanelUI>();
    }

    private void Start()
    {
        if (targetStructure != null)
        {
            _factoryBehavior = targetStructure.GetBehaviour() as FactoryBehavior;
        }

        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        if (collectButton != null) collectButton.onClick.AddListener(OnCollectButtonClicked);

        _panelUI.ShowStarted += UpdateWalletDisplay;

        // Force the text to update immediately on startup!
        UpdateWalletDisplay();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
        if (collectButton != null) collectButton.onClick.RemoveListener(OnCollectButtonClicked);

        if (_panelUI != null) _panelUI.ShowStarted -= UpdateWalletDisplay;
    }

    private void OnUpgradeButtonClicked()
    {
        if (_factoryBehavior != null)
        {
            _factoryBehavior.TryUpgrade();
            UpdateWalletDisplay();
        }
    }

    private void OnCollectButtonClicked()
    {
        if (_factoryBehavior != null)
        {
            _factoryBehavior.CollectResources();
            UpdateWalletDisplay();
        }
    }

    private void UpdateWalletDisplay()
    {
        if (playerWalletText != null && SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            GameData cachedData = SaveManager.Instance.Load();

            // Dynamically check whatever resource this factory costs to upgrade
            float availableResources = 0f;
            switch (_factoryBehavior.upgradeCostType)
            {
                case ResourceType.Gold: availableResources = cachedData.resources.gold; break;
                case ResourceType.Food: availableResources = cachedData.resources.food; break;
                case ResourceType.Energy: availableResources = cachedData.resources.energy; break;
                case ResourceType.Electronics: availableResources = cachedData.resources.electronics; break;
            }

            // Display that specific resource in the UI
            playerWalletText.text = $"Wallet: {availableResources} {_factoryBehavior.upgradeCostType}";
        }
    }

    private void Update()
    {
        if (!_panelUI.IsShown || _factoryBehavior == null) return;

        if (resourceCountText != null)
        {
            int currentFloor = Mathf.FloorToInt(_factoryBehavior.state.currentAccumulated);
            resourceCountText.text = $"{currentFloor} / {Mathf.FloorToInt(_factoryBehavior.maxStorageCapacity)}";
        }

        if (statusSubfillText != null)
        {
            statusSubfillText.text = $"+{_factoryBehavior.productionRatePerSecond:F1} {_factoryBehavior.state.resourceType}/s (LVL {_factoryBehavior.state.currentLevel})";
        }

        if (upgradeButtonText != null)
        {
            int cost = _factoryBehavior.GetUpgradeCost();
            upgradeButtonText.text = $"Upgrade (Cost: {cost} {_factoryBehavior.upgradeCostType})";
        }
    }
}