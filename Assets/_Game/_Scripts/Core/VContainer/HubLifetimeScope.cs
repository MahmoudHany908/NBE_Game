using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HubLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private GateTrigger gateTrigger;
    [SerializeField] private FactoryBehavior factoryBehavior;
    [SerializeField] private FactoryUIDisplay factoryUIDisplay;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("[HubLifetimeScope] Configuring scope...");

        // Only register if they are assigned!
        if (settingsMenu != null) builder.RegisterComponent(settingsMenu);
        if (gateTrigger != null) builder.RegisterComponent(gateTrigger);
        if (factoryBehavior != null) builder.RegisterComponent(factoryBehavior);
        if (factoryUIDisplay != null) builder.RegisterComponent(factoryUIDisplay);

        // Force explicit injection on the instances
        builder.RegisterBuildCallback(container =>
        {
            Debug.Log("[HubLifetimeScope] Scope built! Manually injecting scene components...");

            if (settingsMenu != null) container.Inject(settingsMenu);
            else Debug.LogWarning("[HubLifetimeScope] SettingsMenu is not assigned in the inspector!");

            if (gateTrigger != null) container.Inject(gateTrigger);
            else Debug.LogWarning("[HubLifetimeScope] GateTrigger is not assigned in the inspector!");

            if (factoryBehavior != null) container.Inject(factoryBehavior);
            else Debug.LogWarning("[HubLifetimeScope] FactoryBehavior is not assigned in the inspector!");

            if (factoryUIDisplay != null) container.Inject(factoryUIDisplay);
            else Debug.LogWarning("[HubLifetimeScope] FactoryUIDisplay is not assigned in the inspector!");
        });
    }
}
