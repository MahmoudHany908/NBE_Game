using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HubLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private GateTrigger gateTrigger;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("[HubLifetimeScope] Configuring scope...");

        // Only register if they are assigned!
        if (settingsMenu != null) builder.RegisterComponent(settingsMenu);
        if (gateTrigger != null) builder.RegisterComponent(gateTrigger);

        // Force explicit injection on the instances
        builder.RegisterBuildCallback(container =>
        {
            Debug.Log("[HubLifetimeScope] Scope built! Manually injecting scene components...");

            if (settingsMenu != null) container.Inject(settingsMenu);
            else Debug.LogWarning("[HubLifetimeScope] SettingsMenu is not assigned in the inspector!");

            if (gateTrigger != null) container.Inject(gateTrigger);
            else Debug.LogWarning("[HubLifetimeScope] GateTrigger is not assigned in the inspector!");
        });
    }
}
