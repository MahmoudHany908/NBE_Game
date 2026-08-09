using UnityEngine;
using VContainer;
using VContainer.Unity;
using VaultSurvivors.UI;

public class HubLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private StatsMenuUIController statsMenu;
    [SerializeField] private UnityEngine.UI.Button statsButton;
    [SerializeField] private GateTrigger gateTrigger;
    [SerializeField] private FactoryBehavior factoryBehavior;
    [SerializeField] private MaterialsHUDController _materialsHUDController;

    [Header("Graphics")]
    [SerializeField] private GraphicsSettingsManager _graphicsSettingsManager;
    [SerializeField] private GraphicsQualityUI _graphicsQualityUI;

    [SerializeField] private FactoryControlPanel[] _factoryControlPanels;
    private FactoryUIController _factoryUIController;

    private CombatWorkshopControlPanel _combatWorkshopControlPanel;
    private CombatWorkshopUIController _combatWorkshopUIController;
    
    [SerializeField] private GateLoadoutUIController _gateLoadoutUIController;

    private MarketUIController _marketUIController;
    private MarketQuantityModal _marketQuantityModal;
    private MarketConfirmModal _marketConfirmModal;
    private MarketControlPanel _marketControlPanel;

    private VaultSurvivors.Core.Production.FactoryCinematicController _factoryCinematicController;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("[HubLifetimeScope] Configuring scope...");

        _factoryUIController = FindAnyObjectByType<FactoryUIController>(FindObjectsInactive.Include);
        var uiManager = FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);

        _combatWorkshopControlPanel = FindAnyObjectByType<CombatWorkshopControlPanel>(FindObjectsInactive.Include);
        _combatWorkshopUIController = FindAnyObjectByType<CombatWorkshopUIController>(FindObjectsInactive.Include);
        
        gateTrigger = FindAnyObjectByType<GateTrigger>(FindObjectsInactive.Include);

        _marketUIController = FindAnyObjectByType<MarketUIController>(FindObjectsInactive.Include);
        _marketQuantityModal = FindAnyObjectByType<MarketQuantityModal>(FindObjectsInactive.Include);
        _marketConfirmModal = FindAnyObjectByType<MarketConfirmModal>(FindObjectsInactive.Include);
        _marketControlPanel = FindAnyObjectByType<MarketControlPanel>(FindObjectsInactive.Include);

        _factoryCinematicController = FindAnyObjectByType<VaultSurvivors.Core.Production.FactoryCinematicController>(FindObjectsInactive.Include);

        if (settingsMenu != null) builder.RegisterComponent(settingsMenu);
        if (statsMenu != null) builder.RegisterComponent(statsMenu);
        if (gateTrigger != null) builder.RegisterComponent(gateTrigger);
        if (factoryBehavior != null) builder.RegisterComponent(factoryBehavior);

        // Graphics Quality System
        if (_graphicsSettingsManager != null) builder.RegisterComponent(_graphicsSettingsManager);
        if (_graphicsQualityUI != null) builder.RegisterComponent(_graphicsQualityUI);

        if (_factoryUIController != null) builder.RegisterComponent(_factoryUIController);
        if (_materialsHUDController != null) builder.RegisterComponent(_materialsHUDController);
        if (uiManager != null) builder.RegisterComponent(uiManager);

        if (_combatWorkshopControlPanel != null) builder.RegisterComponent(_combatWorkshopControlPanel);
        if (_combatWorkshopUIController != null) builder.RegisterComponent(_combatWorkshopUIController);
        if (_gateLoadoutUIController != null) builder.RegisterComponent(_gateLoadoutUIController);

        if (_marketUIController != null) builder.RegisterComponent(_marketUIController);
        if (_marketQuantityModal != null) builder.RegisterComponent(_marketQuantityModal);
        if (_marketConfirmModal != null) builder.RegisterComponent(_marketConfirmModal);
        if (_marketControlPanel != null) builder.RegisterComponent(_marketControlPanel);
        
        if (_factoryCinematicController != null) builder.RegisterComponent(_factoryCinematicController);

        builder.RegisterBuildCallback(container =>
        {
            if (settingsMenu != null) container.Inject(settingsMenu);
            if (statsMenu != null) container.Inject(statsMenu);
            if (gateTrigger != null) container.Inject(gateTrigger);
            if (factoryBehavior != null) container.Inject(factoryBehavior);

            if (_graphicsSettingsManager != null)
            {
                container.Inject(_graphicsSettingsManager);
                Debug.Log("[GraphicsSettingsManager] Successfully Injected!");
            }
            if (_graphicsQualityUI != null)
            {
                container.Inject(_graphicsQualityUI);
                Debug.Log("[GraphicsQualityUI] Successfully Injected!");
            }

            // Wire the settings button click to open the settings menu (no Inspector UnityEvents)
            if (settingsButton != null && settingsMenu != null)
                settingsButton.onClick.AddListener(settingsMenu.OpenSettingsMenu);

            if (statsButton != null && statsMenu != null)
                statsButton.onClick.AddListener(statsMenu.OpenMenu);

            if (_factoryControlPanels != null && _factoryControlPanels.Length > 0)
            {
                foreach (var panel in _factoryControlPanels)
                {
                    container.Inject(panel);
                }
                Debug.Log($"[FactoryControlPanels] Successfully Injected {_factoryControlPanels.Length} panels!");
            }
            else
            {
                Debug.LogError("[HubLifetimeScope] FactoryControlPanel NOT FOUND in scene!");
            }

            if (_factoryUIController != null)
            {
                container.Inject(_factoryUIController);
                Debug.Log("[FactoryUIController] Successfully Injected!");
            }

            if (uiManager != null)
            {
                container.Inject(uiManager);
                Debug.Log("[UIManager] Successfully Injected!");
            }

            if (_materialsHUDController != null)
            {
                container.Inject(_materialsHUDController);
                Debug.Log("[MaterialsHUDController] Successfully Injected!");
            }

            if (_combatWorkshopControlPanel != null)
            {
                container.Inject(_combatWorkshopControlPanel);
                Debug.Log("[CombatWorkshopControlPanel] Successfully Injected!");
            }

            if (_combatWorkshopUIController != null)
            {
                container.Inject(_combatWorkshopUIController);
                Debug.Log("[CombatWorkshopUIController] Successfully Injected!");
            }
            
            if (_gateLoadoutUIController != null)
            {
                container.Inject(_gateLoadoutUIController);
                Debug.Log("[GateLoadoutUIController] Successfully Injected!");
            }

            if (_marketUIController != null)
            {
                container.Inject(_marketUIController);
                Debug.Log("[MarketUIController] Successfully Injected!");
            }

            if (_marketQuantityModal != null)
            {
                container.Inject(_marketQuantityModal);
                Debug.Log("[MarketQuantityModal] Successfully Injected!");
            }

            if (_marketConfirmModal != null)
            {
                container.Inject(_marketConfirmModal);
                Debug.Log("[MarketConfirmModal] Successfully Injected!");
            }

            if (_marketControlPanel != null)
            {
                container.Inject(_marketControlPanel);
                Debug.Log("[MarketControlPanel] Successfully Injected!");
            }

            if (_factoryCinematicController != null)
            {
                container.Inject(_factoryCinematicController);
                Debug.Log("[FactoryCinematicController] Successfully Injected!");
            }

            var globalAudio = GetComponent<GlobalUIButtonAudio>();
            if (globalAudio != null)
            {
                container.Inject(globalAudio);
                Debug.Log("[GlobalUIButtonAudio] Successfully Injected!");
            }
        });
    }
}