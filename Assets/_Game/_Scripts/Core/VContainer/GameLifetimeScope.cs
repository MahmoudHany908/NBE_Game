using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameStateManager gameStateManager;

    protected override void Configure(IContainerBuilder builder)
    {
        // Register Managers
        builder.RegisterComponent(audioManager);
        builder.RegisterComponent(gameStateManager).As<IGameStateManager>();

        // Register PoolManager as a singleton so it persists globally
        builder.RegisterComponent(poolManager).AsSelf();

        // Register Event System
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventPublisher>().As<IEventSubscriber>();

        // Force injection on components that are not resolved by others
        builder.RegisterBuildCallback(container => 
        {
            container.Resolve<IGameStateManager>();
        });
    }
}