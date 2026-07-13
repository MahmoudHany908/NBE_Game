using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HubLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private GateTrigger gateTrigger;
    [SerializeField] private FactoryBehavior factoryBehavior;

    private FactoryControlPanel _factoryControlPanel;
    private FactoryUIController _factoryUIController;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("[HubLifetimeScope] Configuring scope...");

        _factoryControlPanel = FindAnyObjectByType<FactoryControlPanel>();
        _factoryUIController = FindAnyObjectByType<FactoryUIController>();

        if (settingsMenu != null) builder.RegisterComponent(settingsMenu);
        if (gateTrigger != null) builder.RegisterComponent(gateTrigger);
        if (factoryBehavior != null) builder.RegisterComponent(factoryBehavior);

        if (_factoryControlPanel != null) builder.RegisterComponent(_factoryControlPanel);
        if (_factoryUIController != null) builder.RegisterComponent(_factoryUIController);

        builder.RegisterBuildCallback(container =>
        {
            if (settingsMenu != null) container.Inject(settingsMenu);
            if (gateTrigger != null) container.Inject(gateTrigger);
            if (factoryBehavior != null) container.Inject(factoryBehavior);

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
        });
    }
}