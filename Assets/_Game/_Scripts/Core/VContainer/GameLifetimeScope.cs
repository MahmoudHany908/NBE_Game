using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AudioManager audioManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(audioManager);
        builder.RegisterComponent(poolManager);

        // Register managers
        builder.RegisterComponentInHierarchy<GameStateManager>().As<IGameStateManager>();
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventPublisher>().As<IEventSubscriber>();

        // Add these lines to handle the UI Toolkit scripts in the scene
        builder.RegisterComponentInHierarchy<SettingsMenu>();
        builder.RegisterComponentInHierarchy<SceneTransitionView>(); // <--- ADD THIS LINE
    }
}