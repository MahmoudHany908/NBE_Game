using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private VaultSurvivors.LoadingSystem.LoadingScreenController loadingScreenController;

    protected override void Configure(IContainerBuilder builder)
    {
        if (loadingScreenController == null)
        {
            loadingScreenController = FindAnyObjectByType<VaultSurvivors.LoadingSystem.LoadingScreenController>();
            if (loadingScreenController == null)
            {
                Debug.LogWarning("[GameLifetimeScope] LoadingScreenController not found in the scene! Injection will fail.");
            }
        }

        // Register Managers
        builder.RegisterComponent(audioManager);
        builder.RegisterComponent(gameStateManager).As<IGameStateManager>();
        builder.RegisterComponent(loadingScreenController);

        builder.RegisterEntryPoint<VaultSurvivors.Core.Production.ProductionManager>().As<VaultSurvivors.Core.Production.IProductionManager>();

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
            container.Inject(loadingScreenController);
        });
    }
}