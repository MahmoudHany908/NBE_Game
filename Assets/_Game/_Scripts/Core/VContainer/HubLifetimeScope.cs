using UnityEngine;
using VContainer;
using VContainer.Unity;
using VaultSurvivors.UI;

public class HubLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private GateTrigger gateTrigger;
    [SerializeField] private FactoryBehavior factoryBehavior;

    private FactoryControlPanel _factoryControlPanel;
    private FactoryUIController _factoryUIController;
    private MaterialsHUDController _materialsHUDController;

    private CombatWorkshopControlPanel _combatWorkshopControlPanel;
    private CombatWorkshopUIController _combatWorkshopUIController;
    
    [SerializeField] private GateLoadoutUIController _gateLoadoutUIController;

    private MarketUIController _marketUIController;
    private MarketQuantityModal _marketQuantityModal;
    private MarketConfirmModal _marketConfirmModal;
    private MarketControlPanel _marketControlPanel;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("[HubLifetimeScope] Configuring scope...");

        _factoryControlPanel = FindAnyObjectByType<FactoryControlPanel>(FindObjectsInactive.Include);
        _factoryUIController = FindAnyObjectByType<FactoryUIController>(FindObjectsInactive.Include);
        _materialsHUDController = FindAnyObjectByType<MaterialsHUDController>(FindObjectsInactive.Include);
        var uiManager = FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);

        _combatWorkshopControlPanel = FindAnyObjectByType<CombatWorkshopControlPanel>(FindObjectsInactive.Include);
        _combatWorkshopUIController = FindAnyObjectByType<CombatWorkshopUIController>(FindObjectsInactive.Include);
        
        gateTrigger = FindAnyObjectByType<GateTrigger>(FindObjectsInactive.Include);

        _marketUIController = FindAnyObjectByType<MarketUIController>(FindObjectsInactive.Include);
        _marketQuantityModal = FindAnyObjectByType<MarketQuantityModal>(FindObjectsInactive.Include);
        _marketConfirmModal = FindAnyObjectByType<MarketConfirmModal>(FindObjectsInactive.Include);
        _marketControlPanel = FindAnyObjectByType<MarketControlPanel>(FindObjectsInactive.Include);

        if (settingsMenu != null) builder.RegisterComponent(settingsMenu);
        if (gateTrigger != null) builder.RegisterComponent(gateTrigger);
        if (factoryBehavior != null) builder.RegisterComponent(factoryBehavior);

        if (_factoryControlPanel != null) builder.RegisterComponent(_factoryControlPanel);
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

        builder.RegisterBuildCallback(container =>
        {
            if (settingsMenu != null) container.Inject(settingsMenu);
            if (gateTrigger != null) container.Inject(gateTrigger);
            if (factoryBehavior != null) container.Inject(factoryBehavior);

            // Wire the settings button click to open the settings menu (no Inspector UnityEvents)
            if (settingsButton != null && settingsMenu != null)
                settingsButton.onClick.AddListener(settingsMenu.OpenSettingsMenu);

            if (_factoryControlPanel != null)
            {
                container.Inject(_factoryControlPanel);
                Debug.Log("[FactoryControlPanel] Successfully Injected!");
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
        });
    }
}