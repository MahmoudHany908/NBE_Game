using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private VaultSurvivors.LoadingSystem.LoadingScreenController loadingScreenController;
    [SerializeField] private SupportCharacterRegistrySO supportCharacterRegistry;

    protected override void Configure(IContainerBuilder builder)
    {
        if (loadingScreenController == null)
        {
            loadingScreenController = FindAnyObjectByType<VaultSurvivors.LoadingSystem.LoadingScreenController>();
            if (loadingScreenController == null)
            {
                //Debug.LogWarning("[GameLifetimeScope] LoadingScreenController not found in the scene! Injection will fail.");
            }
        }

        // Register Managers
        builder.RegisterComponent(audioManager);
        builder.RegisterComponent(gameStateManager).As<IGameStateManager>();
        builder.RegisterComponent(loadingScreenController);

        if (supportCharacterRegistry != null)
        {
            builder.RegisterInstance(supportCharacterRegistry);
        }
        else
        {
            //Debug.LogWarning("[GameLifetimeScope] SupportCharacterRegistrySO is not assigned in the inspector!");
        }

        builder.RegisterEntryPoint<VaultSurvivors.Core.Production.ProductionManager>().As<VaultSurvivors.Core.Production.IProductionManager>();
        builder.RegisterEntryPoint<VaultSurvivors.Core.Market.MarketManager>().As<VaultSurvivors.Core.Market.IMarketManager>();
        builder.RegisterEntryPoint<CombatWorkshopManager>().As<ICombatWorkshopManager>();

        // Register PoolManager as a singleton so it persists globally
        builder.RegisterComponent(poolManager).AsSelf();

        // Register Event System
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventPublisher>().As<IEventSubscriber>();

        // Register Save System
        builder.Register<SaveManager>(Lifetime.Singleton).As<ISaveSystem>();

        // Register Time System
        builder.Register<VaultSurvivors.Core.Time.TimeManager>(Lifetime.Singleton)
            .As<VaultSurvivors.Core.Time.ITimeManager>();

        // Force injection on components that are not resolved by others
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<IGameStateManager>();
            container.Resolve<VaultSurvivors.Core.Time.ITimeManager>();
            container.Inject(loadingScreenController);
        });
    }
}
